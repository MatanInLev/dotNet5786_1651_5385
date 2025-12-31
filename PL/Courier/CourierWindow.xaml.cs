using BlApi;
using BO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace PL.Courier
{
    public partial class CourierWindow : Window
    {
        static readonly IBl s_bl = Factory.Get();

        private BO.Courier? _originalCourier; // the source object from BL (for existing courier)

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
                // Load original from BL and edit a copy so closing/cancel doesn't commit changes
                _originalCourier = s_bl.Courier.Get(adminId, courierId);
                CurrentCourier = CloneCourier(_originalCourier!);
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
                    StartWorkDate = s_bl.Admin.GetClock()
                };
            }

            // Update UI element states based on CurrentCourier
            UpdateUIState();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
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
                var result = MessageBox.Show($"Are you sure you want to delete courier {CurrentCourier.Id}?",
                                             "Confirm Delete",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

                int adminId = s_bl.Admin.GetConfig().AdminId;
                s_bl.Courier.Delete(adminId, CurrentCourier.Id);
                MessageBox.Show("Courier deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (BlInvalidValueException ex)
            {
                MessageBox.Show($"Invalid Data: {ex.Message}", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the Add/Update button click.
        /// Commits only when the user clicks the button (save-on-demand).
        /// </summary>
        private void btnAction_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentCourier == null) return;

                // Local validation before calling BL to give faster feedback
                var validationError = ValidateBeforeSave(CurrentCourier, ButtonText == "Add");
                if (validationError != null)
                {
                    MessageBox.Show(validationError, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int adminId = s_bl.Admin.GetConfig().AdminId;

                if (ButtonText == "Add")
                {
                    s_bl.Courier.Add(adminId, CurrentCourier);

                    MessageBox.Show("Courier added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    s_bl.Courier.Update(adminId, CurrentCourier);
                    MessageBox.Show("Courier updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                // Refresh UI state after operation
                UpdateUIState();
                Close();
            }
            catch (BlInvalidValueException ex)
            {
                MessageBox.Show($"Invalid Data: {ex.Message}", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (BlAlreadyExistsException ex)
            {
                MessageBox.Show($"ID already exists: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CourierObserver()
        {
            Dispatcher.Invoke(() =>
            {
                if (CurrentCourier?.Id == 0) return;

                try
                {
                    int adminId = s_bl.Admin.GetConfig().AdminId; 
                    CurrentCourier = s_bl.Courier.Get(adminId, CurrentCourier!.Id);
                    UpdateUIState();
                }
                catch (Exception)
                {
                    Close();
                }
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (CurrentCourier != null && CurrentCourier.Id != 0)
            {
                s_bl.Courier.AddObserver(CurrentCourier.Id, CourierObserver);
            }
        }
        private void Window_Closed(object? sender, EventArgs e)
        {
            if (CurrentCourier != null && CurrentCourier.Id != 0)
            {
                s_bl.Courier.RemoveObserver(CurrentCourier.Id, CourierObserver);
            }
        }

        private BO.Courier CloneCourier(BO.Courier src)
        {
            if (src == null) return new BO.Courier() { Id = 0, IsActive = true, Vehicle = BO.Vehicle.None, StartWorkDate = s_bl.Admin.GetClock() };

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {

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

        /// <summary>
        /// Update UI enabled/readonly states based on CurrentCourier values.
        /// </summary>
        private void UpdateUIState()
        {
            // If controls not yet loaded, nothing to do
            if (!IsLoaded) return;

            // Delete enabled only for existing courier (Id != 0)
            if (btnDelete != null)
                btnDelete.IsEnabled = (CurrentCourier != null && CurrentCourier.Id != 0);

            // Vehicle selection disabled if courier currently has an order in progress
            if (cmbVehicle != null)
                cmbVehicle.IsEnabled = !(CurrentCourier?.OrderInProgress != null);

            // ID TextBox read-only already bound to IsIdReadOnly property
        }
    }
}