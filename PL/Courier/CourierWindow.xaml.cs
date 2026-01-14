using BlApi;
using BO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace PL.Courier
{
    public partial class CourierWindow : Window, IDisposable
    {
        static readonly IBl s_bl = Factory.Get();

        /// <summary>
        /// Observer mutex to prevent concurrent observer callbacks - Stage 7
        /// </summary>
        private readonly ObserverMutex _observerMutex = new(); //stage 7

        private BO.Courier? _originalCourier;
        private bool _observerRegistered = false;
        private bool _disposed = false;

        #region Dependency Properties

        // Property for the text color (White for Delete, Black for Cancel)
        public System.Windows.Media.Brush DeleteButtonForeground
        {
            get { return (System.Windows.Media.Brush)GetValue(DeleteButtonForegroundProperty); }
            set { SetValue(DeleteButtonForegroundProperty, value); }
        }

        public static readonly DependencyProperty DeleteButtonForegroundProperty =
            DependencyProperty.Register(nameof(DeleteButtonForeground), typeof(System.Windows.Media.Brush), typeof(CourierWindow), new PropertyMetadata(System.Windows.Media.Brushes.White));

        // Property for the Delete/Cancel button text
        public string DeleteButtonText
        {
            get { return (string)GetValue(DeleteButtonTextProperty); }
            set { SetValue(DeleteButtonTextProperty, value); }
        } 

        public static readonly DependencyProperty DeleteButtonTextProperty =
            DependencyProperty.Register(nameof(DeleteButtonText), typeof(string), typeof(CourierWindow), new PropertyMetadata("Delete Courier"));

        // Property for the button background color (so Cancel isn't Red)
        public System.Windows.Media.Brush DeleteButtonBackground
        {
            get { return (System.Windows.Media.Brush)GetValue(DeleteButtonBackgroundProperty); }
            set { SetValue(DeleteButtonBackgroundProperty, value); }
        }

        public static readonly DependencyProperty DeleteButtonBackgroundProperty =
            DependencyProperty.Register(nameof(DeleteButtonBackground), typeof(System.Windows.Media.Brush), typeof(CourierWindow), new PropertyMetadata(System.Windows.Media.Brushes.Red));
        public BO.Courier? CurrentCourier
        {
            get { return (BO.Courier?)GetValue(CurrentCourierProperty); }
            set { SetValue(CurrentCourierProperty, value); }
        }

        public static readonly DependencyProperty CurrentCourierProperty =
            DependencyProperty.Register(nameof(CurrentCourier), typeof(BO.Courier), typeof(CourierWindow), new PropertyMetadata(null));

        public string ButtonText
        {
            get { return (string)GetValue(ButtonTextProperty); }
            set { SetValue(ButtonTextProperty, value); }
        }

        public static readonly DependencyProperty ButtonTextProperty =
            DependencyProperty.Register(nameof(ButtonText), typeof(string), typeof(CourierWindow), new PropertyMetadata("Action"));

        // Property to populate the ComboBox with Enum values
        public IEnumerable<BO.Vehicle> VehicleOptions { get; } = Enum.GetValues(typeof(BO.Vehicle)).Cast<BO.Vehicle>();

        // New DP: control whether the ID TextBox is read-only (existing courier)
        public bool IsIdReadOnly
        {
            get => (bool)GetValue(IsIdReadOnlyProperty);
            set => SetValue(IsIdReadOnlyProperty, value);
        }

        public static readonly DependencyProperty IsIdReadOnlyProperty =
            DependencyProperty.Register(nameof(IsIdReadOnly), typeof(bool), typeof(CourierWindow), new PropertyMetadata(false));

        #endregion

        public CourierWindow(int courierId = 0)
        {
            int adminId = s_bl.Admin.GetConfig().AdminId;

            (ButtonText, DeleteButtonText, DeleteButtonBackground, DeleteButtonForeground) = (courierId == 0)
            ? ("Add", "Cancel", System.Windows.Media.Brushes.LightGray, System.Windows.Media.Brushes.Black)
            : ("Update", "Delete Courier", System.Windows.Media.Brushes.Red, System.Windows.Media.Brushes.White);

            InitializeComponent();
            Closed += Window_Closed;
            Loaded += Window_Loaded;

            // If editing existing courier, make ID readonly in UI
            IsIdReadOnly = courierId != 0;

            if (courierId != 0)
            {
                // Load data asynchronously to avoid UI freeze
                Loaded += async (s, e) =>
                {
                    await LoadCourierDataAsync(adminId, courierId);
                };
            }
            else
            {
                // New courier: create editing instance with sensible defaults
                _originalCourier = null;
                CurrentCourier = new BO.Courier()
                {
                    Id = 0,
                    IsActive = true,
                    Vehicle = BO.Vehicle.None,
                    StartWorkDate = DateTime.Today
                };
            }
        }

        private async System.Threading.Tasks.Task LoadCourierDataAsync(int adminId, int courierId)
        {
            try
            {
                BO.Courier? courier = null;
                
                await System.Threading.Tasks.Task.Run(() =>
                {
                    courier = s_bl.Courier.Get(adminId, courierId);
                });
                
                _originalCourier = courier;
                CurrentCourier = CloneCourier(_originalCourier!);
            }
            catch (Exception ex)
            {
                ModernMessageBox.Show($"Error loading courier: {ex.Message}", "Error", 
                    ModernMessageBox.MessageBoxType.Error, ModernMessageBox.MessageBoxButtons.OK, this);
                Close();
            }
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentCourier == null) return;
                if (CurrentCourier.Id == 0)
                {
                    // New courier -> Cancel
                    Close();
                    return;
                }

                // Confirm with the user before attempting deletion
                var result = ModernMessageBox.Show($"Are you sure you want to delete courier {CurrentCourier.Id}?",
                                             "Confirm Delete",
                                             ModernMessageBox.MessageBoxType.Question,
                                             ModernMessageBox.MessageBoxButtons.YesNo,
                                             this);

                if (result != true)
                    return;

                // Disable button during operation
                var button = sender as System.Windows.Controls.Button;
                if (button != null)
                {
                    button.IsEnabled = false;
                }

                int adminId = s_bl.Admin.GetConfig().AdminId;
                int courierIdToDelete = CurrentCourier.Id;

                await System.Threading.Tasks.Task.Run(() =>
                {
                    s_bl.Courier.Delete(adminId, courierIdToDelete);
                });

                ModernMessageBox.Show("Courier deleted successfully!", "Success", ModernMessageBox.MessageBoxType.Success, ModernMessageBox.MessageBoxButtons.OK, this);
                Close();
            }
            catch (BlInvalidValueException ex)
            {
                ModernMessageBox.Show($"Invalid Data: {ex.Message}", "Validation Error", ModernMessageBox.MessageBoxType.Warning, ModernMessageBox.MessageBoxButtons.OK, this);
                
                var button = sender as System.Windows.Controls.Button;
                if (button != null)
                {
                    button.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                ModernMessageBox.Show($"Error: {ex.Message}", "Error", ModernMessageBox.MessageBoxType.Error, ModernMessageBox.MessageBoxButtons.OK, this);
                
                var button = sender as System.Windows.Controls.Button;
                if (button != null)
                {
                    button.IsEnabled = true;
                }
            }
        }

        /// <summary>
        /// Handles the Add/Update button click.
        /// Commits only when the user clicks the button (save-on-demand).
        /// </summary>
        private async void btnAction_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentCourier == null) return;

                // Local validation before calling BL to give faster feedback
                var validationError = ValidateBeforeSave(CurrentCourier, ButtonText == "Add");
                if (validationError != null)
                {
                    ModernMessageBox.Show(validationError, "Validation Error", ModernMessageBox.MessageBoxType.Warning, ModernMessageBox.MessageBoxButtons.OK, this);
                    return;
                }

                // Disable button during operation
                var button = sender as System.Windows.Controls.Button;
                if (button != null)
                {
                    button.IsEnabled = false;
                }

                int adminId = s_bl.Admin.GetConfig().AdminId;

                if (ButtonText == "Add")
                {
                    // Create a local copy to avoid cross-thread access issues
                    var courierToAdd = CurrentCourier;

                    await System.Threading.Tasks.Task.Run(() =>
                    {
                        s_bl.Courier.Add(adminId, courierToAdd);
                    });

                    ModernMessageBox.Show("Courier added successfully!", "Success", ModernMessageBox.MessageBoxType.Success, ModernMessageBox.MessageBoxButtons.OK, this);
                }
                else
                {
                    // Check if courier is being deactivated and has active deliveries
                    if (_originalCourier != null && _originalCourier.IsActive && !CurrentCourier.IsActive)
                    {
                        if (CurrentCourier.OrderInProgress != null)
                        {
                            var result = ModernMessageBox.Show(
                                $"This courier is currently delivering Order #{CurrentCourier.OrderInProgress.OrderId}.\n\n" +
                                "Deactivating will cancel this active delivery.\n\n" +
                                "Are you sure you want to continue?",
                                "Confirm Deactivation",
                                ModernMessageBox.MessageBoxType.Warning,
                                ModernMessageBox.MessageBoxButtons.YesNo,
                                this);

                            if (result != true)
                            {
                                // Re-enable button if user cancelled
                                if (button != null)
                                {
                                    button.IsEnabled = true;
                                }
                                return;
                            }
                        }
                        else
                        {
                            var result = ModernMessageBox.Show(
                                $"Are you sure you want to deactivate courier {CurrentCourier.Name}?",
                                "Confirm Deactivation",
                                ModernMessageBox.MessageBoxType.Question,
                                ModernMessageBox.MessageBoxButtons.YesNo,
                                this);

                            if (result != true)
                            {
                                // Re-enable button if user cancelled
                                if (button != null)
                                {
                                    button.IsEnabled = true;
                                }
                                return;
                            }
                        }
                    }

                    // Create a local copy to avoid cross-thread access issues
                    var courierToUpdate = CurrentCourier;

                    await System.Threading.Tasks.Task.Run(() =>
                    {
                        s_bl.Courier.Update(adminId, courierToUpdate);
                    });

                    ModernMessageBox.Show("Courier updated successfully!", "Success", ModernMessageBox.MessageBoxType.Success, ModernMessageBox.MessageBoxButtons.OK, this);
                }
                Close();
            }
            catch (BlDoesNotExistException ex)
            {
                ModernMessageBox.Show($"Not Found: {ex.Message}", "Error", ModernMessageBox.MessageBoxType.Warning, ModernMessageBox.MessageBoxButtons.OK, this);
                
                var button = sender as System.Windows.Controls.Button;
                if (button != null)
                {
                    button.IsEnabled = true;
                }
            }
            catch (BlInvalidValueException ex)
            {
                ModernMessageBox.Show($"Invalid Data: {ex.Message}", "Validation Error", ModernMessageBox.MessageBoxType.Warning, ModernMessageBox.MessageBoxButtons.OK, this);
                
                var button = sender as System.Windows.Controls.Button;
                if (button != null)
                {
                    button.IsEnabled = true;
                }
            }
            catch (BlAlreadyExistsException ex)
            {
                ModernMessageBox.Show($"ID Already Exists: {ex.Message}", "Error", ModernMessageBox.MessageBoxType.Error, ModernMessageBox.MessageBoxButtons.OK, this);
                
                var button = sender as System.Windows.Controls.Button;
                if (button != null)
                {
                    button.IsEnabled = true;
                }
            }
            catch (BlBaseException ex)
            {
                ModernMessageBox.Show($"Business Logic Error: {ex.Message}", "Error", ModernMessageBox.MessageBoxType.Error, ModernMessageBox.MessageBoxButtons.OK, this);
                
                var button = sender as System.Windows.Controls.Button;
                if (button != null)
                {
                    button.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                ModernMessageBox.Show($"An unexpected error occurred. Please try again or contact support.\n\nDetails: {ex.Message}", 
                    "Unexpected Error", ModernMessageBox.MessageBoxType.Error, ModernMessageBox.MessageBoxButtons.OK, this);
                
                var button = sender as System.Windows.Controls.Button;
                if (button != null)
                {
                    button.IsEnabled = true;
                }
            }
        }

        private void CourierObserver()
        {
            // Stage 7: Check if already processing - if so, exit immediately
            if (_observerMutex.CheckAndSetInProgress())
                return;

            System.Diagnostics.Debug.WriteLine($"[CourierWindow] Observer fired for courier {CurrentCourier?.Id} at {DateTime.Now:HH:mm:ss.fff}");

            // Use InvokeAsync to avoid blocking the BL thread (prevent deadlock)
            Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    if (CurrentCourier?.Id == 0) return;

                    try
                    {
                        int adminId = s_bl.Admin.GetConfig().AdminId;
                        BO.Courier? updatedCourier = null;

                        await System.Threading.Tasks.Task.Run(() =>
                        {
                            updatedCourier = s_bl.Courier.Get(adminId, CurrentCourier!.Id);
                        });

                        System.Diagnostics.Debug.WriteLine($"[CourierWindow] Refreshing courier {CurrentCourier.Id}: {updatedCourier?.Name}");

                        // Replace the entire object and update the original
                        _originalCourier = updatedCourier;
                        CurrentCourier = CloneCourier(updatedCourier!);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[CourierWindow] Error in observer: {ex.Message}");
                        // Courier was deleted or no longer accessible
                        Close();
                    }
                }
                finally
                {
                    _observerMutex.UnsetInProgress();
                }
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (CurrentCourier != null && CurrentCourier.Id != 0)
            {
                System.Diagnostics.Debug.WriteLine($"[CourierWindow] Registering observer for courier {CurrentCourier.Id}");
                (s_bl.Courier as IObservable)?.AddObserver(CurrentCourier.Id, CourierObserver);
                _observerRegistered = true;
            }
        }
        private void Window_Closed(object? sender, EventArgs e)
        {
            Dispose();
        }

        private BO.Courier CloneCourier(BO.Courier src)
        {
            if (src == null) return new BO.Courier() { Id = 0, IsActive = true, Vehicle = BO.Vehicle.None, StartWorkDate = DateTime.Today };

            return new BO.Courier()
            {
                Id = src.Id,
                Name = src.Name,
                Phone = src.Phone,
                Email = src.Email,
                IsActive = src.IsActive,
                MaxDistance = src.MaxDistance,
                Vehicle = src.Vehicle,
                StartWorkDate = src.StartWorkDate,
                OrdersProvidedOnTime = src.OrdersProvidedOnTime,
                OrdersProvidedLate = src.OrdersProvidedLate,
                OrderInProgress = src.OrderInProgress
            };
        }

        public void Dispose()
        {
            if (_disposed) return;

            if (_observerRegistered && CurrentCourier != null && CurrentCourier.Id != 0)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[CourierWindow] Removing observer for courier {CurrentCourier.Id}");
                    (s_bl.Courier as IObservable)?.RemoveObserver(CurrentCourier.Id, CourierObserver);
                }
                catch
                {
                    // Silently ignore errors during cleanup
                }
                _observerRegistered = false;
            }

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        ~CourierWindow()
        {
            Dispose();
        }

        /// <summary>
        /// Validate input locally before sending to BL.
        /// BL still performs full validation and will throw if invalid.
        /// Returns null if OK or an error message to show to user.
        /// </summary>
        private string? ValidateBeforeSave(BO.Courier c, bool isAdd)
        {
            if (isAdd)
            {
                if (c.Id <= 0)
                    return "ID (T\"Z) must be a positive integer for new courier.";
            }

            if (string.IsNullOrWhiteSpace(c.Name))
                return "Full name is required.";

            if (string.IsNullOrWhiteSpace(c.Phone))
                return "Phone is required.";

            // Simple phone format: 10 digits starting with '0'
            var phone = c.Phone.Trim();
            if (phone.Length != 10 || phone[0] != '0' || !phone.All(char.IsDigit))
                return "Phone must be 10 digits and start with '0'.";

            if (c.MaxDistance.HasValue && c.MaxDistance < 0)
                return "Max distance cannot be negative.";

            return null;
        }
    }
}