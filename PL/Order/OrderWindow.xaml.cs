using BlApi;
using BO;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace PL.Order
{
    /// <summary>
    /// Interaction logic for OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : Window, IDisposable, INotifyPropertyChanged
    {
        static readonly IBl s_bl = Factory.Get();

        /// <summary>
        /// Observer mutex to prevent concurrent observer callbacks - Stage 7
        /// </summary>
        private readonly ObserverMutex _observerMutex = new(); //stage 7

        private BO.Order? _originalOrder;
        private bool _observerRegistered = false;
        private bool _disposed = false;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Dependency Properties

        // Property for the text color (White for Delete, Black for Cancel)
        public Brush DeleteButtonForeground
        {
            get { return (Brush)GetValue(DeleteButtonForegroundProperty); }
            set { SetValue(DeleteButtonForegroundProperty, value); }
        }

        public static readonly DependencyProperty DeleteButtonForegroundProperty =
            DependencyProperty.Register(nameof(DeleteButtonForeground), typeof(Brush), typeof(OrderWindow), new PropertyMetadata(Brushes.White));

        // Property for the Delete/Cancel button text
        public string DeleteButtonText
        {
            get { return (string)GetValue(DeleteButtonTextProperty); }
            set { SetValue(DeleteButtonTextProperty, value); }
        }

        public static readonly DependencyProperty DeleteButtonTextProperty =
            DependencyProperty.Register(nameof(DeleteButtonText), typeof(string), typeof(OrderWindow), new PropertyMetadata("Close"));

        // Property for the button background color (so Cancel isn't Red)
        public Brush DeleteButtonBackground
        {
            get { return (Brush)GetValue(DeleteButtonBackgroundProperty); }
            set { SetValue(DeleteButtonBackgroundProperty, value); }
        }

        public static readonly DependencyProperty DeleteButtonBackgroundProperty =
            DependencyProperty.Register(nameof(DeleteButtonBackground), typeof(Brush), typeof(OrderWindow), new PropertyMetadata(Brushes.Red));

        public BO.Order? CurrentOrder
        {
            get { return (BO.Order?)GetValue(CurrentOrderProperty); }
            set 
            { 
                SetValue(CurrentOrderProperty, value);
                UpdateCoordinatesDisplay();
            }
        }

        public static readonly DependencyProperty CurrentOrderProperty =
            DependencyProperty.Register(nameof(CurrentOrder), typeof(BO.Order), typeof(OrderWindow), new PropertyMetadata(null));

        public string ButtonText
        {
            get { return (string)GetValue(ButtonTextProperty); }
            set { SetValue(ButtonTextProperty, value); }
        }

        public static readonly DependencyProperty ButtonTextProperty =
            DependencyProperty.Register(nameof(ButtonText), typeof(string), typeof(OrderWindow), new PropertyMetadata("Action"));

        #endregion

        #region Properties

        private string _coordinatesDisplay = "Not geocoded";
        public string CoordinatesDisplay
        {
            get => _coordinatesDisplay;
            set
            {
                _coordinatesDisplay = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public OrderWindow(int orderId = 0)
        {
            int adminId = s_bl.Admin.GetConfig().AdminId;

            (ButtonText, DeleteButtonText, DeleteButtonBackground, DeleteButtonForeground) = (orderId == 0)
            ? ("Add", "Cancel", Brushes.LightGray, Brushes.Black)
            : ("Update", "Close", Brushes.LightGray, Brushes.Black);

            InitializeComponent();
            Closed += Window_Closed;
            Loaded += Window_Loaded;

            if (orderId != 0)
            {
                // Load original from BL and edit a copy so closing/cancel doesn't commit changes
                _originalOrder = s_bl.Order.Get(adminId, orderId);
                CurrentOrder = CloneOrder(_originalOrder!);
            }
            else
            {
                // New order: create editing instance with sensible defaults
                _originalOrder = null;
                CurrentOrder = new BO.Order()
                {
                    Id = 0,
                    Type = OrderType.Burger,
                    CustomerAddress = string.Empty,
                    CustomerName = string.Empty,
                    CustomerPhone = string.Empty,
                    OrderDate = s_bl.Admin.GetClock()
                };
            }
        }

        private async void btnValidateAddress_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentOrder == null || string.IsNullOrWhiteSpace(CurrentOrder.CustomerAddress))
            {
                ModernMessageBox.Show("Please enter a delivery address first.", "Validation", 
                    ModernMessageBox.MessageBoxType.Warning, ModernMessageBox.MessageBoxButtons.OK, this);
                return;
            }

            try
            {
                // Show progress
                geocodingProgress.Visibility = Visibility.Visible;
                btnValidateAddress.IsEnabled = false;
                txtAddress.IsEnabled = false;

                // Geocode the address with progress updates
                var (lat, lon) = await Tools.GetCoordinatesAsync(
                    CurrentOrder.CustomerAddress,
                    msg => Dispatcher.Invoke(() => txtGeocodingStatus.Text = msg));

                // Update the order with coordinates
                if (CurrentOrder != null)
                {
                    CurrentOrder = new BO.Order
                    {
                        Id = CurrentOrder.Id,
                        Type = CurrentOrder.Type,
                        Description = CurrentOrder.Description,
                        CustomerAddress = CurrentOrder.CustomerAddress,
                        CustomerName = CurrentOrder.CustomerName,
                        CustomerPhone = CurrentOrder.CustomerPhone,
                        Latitude = lat,
                        Longitude = lon,
                        Distance = CurrentOrder.Distance,
                        OrderDate = CurrentOrder.OrderDate,
                        ExpectedDelivery = CurrentOrder.ExpectedDelivery,
                        MaxDelivery = CurrentOrder.MaxDelivery,
                        Status = CurrentOrder.Status,
                        ScheduleStatus = CurrentOrder.ScheduleStatus,
                        TimeLeft = CurrentOrder.TimeLeft,
                        DeliveryList = CurrentOrder.DeliveryList
                    };

                    UpdateCoordinatesDisplay();
                }

                ModernMessageBox.Show($"✓ Address validated successfully!\n\nCoordinates: ({lat:F6}, {lon:F6})", 
                    "Success", ModernMessageBox.MessageBoxType.Success, ModernMessageBox.MessageBoxButtons.OK, this);
            }
            catch (BlInvalidValueException ex)
            {
                txtGeocodingStatus.Text = "Address not found";
                ModernMessageBox.Show($"Address not found: {ex.Message}\n\nPlease check the address and try again.", 
                    "Geocoding Error", ModernMessageBox.MessageBoxType.Error, ModernMessageBox.MessageBoxButtons.OK, this);
            }
            catch (BlTemporaryNotAvailableException ex)
            {
                txtGeocodingStatus.Text = "Network error";
                ModernMessageBox.Show($"Unable to validate address: {ex.Message}\n\nPlease check your internet connection.", 
                    "Network Error", ModernMessageBox.MessageBoxType.Error, ModernMessageBox.MessageBoxButtons.OK, this);
            }
            catch (Exception ex)
            {
                txtGeocodingStatus.Text = "Error occurred";
                ModernMessageBox.Show($"An error occurred: {ex.Message}", 
                    "Error", ModernMessageBox.MessageBoxType.Error, ModernMessageBox.MessageBoxButtons.OK, this);
            }
            finally
            {
                // Hide progress
                geocodingProgress.Visibility = Visibility.Collapsed;
                btnValidateAddress.IsEnabled = true;
                txtAddress.IsEnabled = true;
            }
        }

        private void UpdateCoordinatesDisplay()
        {
            if (CurrentOrder == null)
            {
                CoordinatesDisplay = "Not geocoded";
                return;
            }

            if (CurrentOrder.Latitude == 0 && CurrentOrder.Longitude == 0)
            {
                CoordinatesDisplay = "Not geocoded (click Validate Address)";
            }
            else
            {
                CoordinatesDisplay = $"Lat: {CurrentOrder.Latitude:F6}, Lon: {CurrentOrder.Longitude:F6}";
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            // This button now acts as "Cancel" for new orders or "Close" for existing orders
            // No deletion happens - orders cannot be deleted per business rules
            Close();
        }

        /// <summary>
        /// Handles the Add/Update button click.
        /// Commits only when the user clicks the button (save-on-demand).
        /// </summary>
        private async void btnAction_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentOrder == null) return;

                int adminId = s_bl.Admin.GetConfig().AdminId;

                // Show geocoding progress if needed
                if (ButtonText == "Add" && (CurrentOrder.Latitude == 0 && CurrentOrder.Longitude == 0))
                {
                    geocodingProgress.Visibility = Visibility.Visible;
                    btnValidateAddress.IsEnabled = false;
                    txtAddress.IsEnabled = false;
                    
                    try
                    {
                        txtGeocodingStatus.Text = "Geocoding address before saving...";
                        // Let the BL handle geocoding, but show progress
                        await System.Threading.Tasks.Task.Delay(500); // Brief delay to show progress
                    }
                    finally
                    {
                        geocodingProgress.Visibility = Visibility.Collapsed;
                        btnValidateAddress.IsEnabled = true;
                        txtAddress.IsEnabled = true;
                    }
                }

                if (ButtonText == "Add")
                {
                    s_bl.Order.Add(adminId, CurrentOrder);
                    ModernMessageBox.Show("Order added successfully!", "Success", 
                        ModernMessageBox.MessageBoxType.Success, ModernMessageBox.MessageBoxButtons.OK, this);
                }
                else
                {
                    s_bl.Order.Update(adminId, CurrentOrder);
                    ModernMessageBox.Show("Order updated successfully!", "Success", 
                        ModernMessageBox.MessageBoxType.Success, ModernMessageBox.MessageBoxButtons.OK, this);
                }
                Close();
            }
            catch (BlDoesNotExistException ex)
            {
                ModernMessageBox.Show($"Not Found: {ex.Message}", "Error", 
                    ModernMessageBox.MessageBoxType.Warning, ModernMessageBox.MessageBoxButtons.OK, this);
            }
            catch (BlInvalidValueException ex)
            {
                ModernMessageBox.Show($"Invalid Data: {ex.Message}", "Validation Error", 
                    ModernMessageBox.MessageBoxType.Warning, ModernMessageBox.MessageBoxButtons.OK, this);
            }
            catch (BlAlreadyExistsException ex)
            {
                ModernMessageBox.Show($"Already Exists: {ex.Message}", "Error", 
                    ModernMessageBox.MessageBoxType.Error, ModernMessageBox.MessageBoxButtons.OK, this);
            }
            catch (BlBaseException ex)
            {
                ModernMessageBox.Show($"Business Logic Error: {ex.Message}", "Error", 
                    ModernMessageBox.MessageBoxType.Error, ModernMessageBox.MessageBoxButtons.OK, this);
            }
            catch (Exception ex)
            {
                ModernMessageBox.Show($"An unexpected error occurred. Please try again or contact support.\n\nDetails: {ex.Message}", 
                    "Unexpected Error", ModernMessageBox.MessageBoxType.Error, ModernMessageBox.MessageBoxButtons.OK, this);
            }
            finally
            {
                geocodingProgress.Visibility = Visibility.Collapsed;
                btnValidateAddress.IsEnabled = true;
                txtAddress.IsEnabled = true;
            }
        }

        private void OrderObserver()
        {
            // Stage 7: Check if already processing - if so, exit immediately
            if (_observerMutex.CheckAndSetInProgress())
                return;

            try
            {
                // Use InvokeAsync to avoid blocking the BL thread (prevent deadlock)
                Dispatcher.InvokeAsync(() =>
                {
                    if (CurrentOrder?.Id == 0) return;

                    try
                    {
                        int adminId = s_bl.Admin.GetConfig().AdminId;
                        var updatedOrder = s_bl.Order.Get(adminId, CurrentOrder!.Id);
                        
                        // Preserve user edits for editable fields by creating a new order with updated calculated fields
                        CurrentOrder = new BO.Order
                        {
                            Id = updatedOrder.Id,
                            Type = CurrentOrder.Type, // Keep user's edit
                            Description = CurrentOrder.Description, // Keep user's edit
                            CustomerAddress = CurrentOrder.CustomerAddress, // Keep user's edit
                            CustomerName = CurrentOrder.CustomerName, // Keep user's edit
                            CustomerPhone = CurrentOrder.CustomerPhone, // Keep user's edit
                            Latitude = updatedOrder.Latitude,
                            Longitude = updatedOrder.Longitude,
                            Distance = updatedOrder.Distance, // Updated from BL
                            OrderDate = updatedOrder.OrderDate,
                            ExpectedDelivery = updatedOrder.ExpectedDelivery, // Updated from BL
                            MaxDelivery = updatedOrder.MaxDelivery, // Updated from BL
                            Status = updatedOrder.Status, // Updated from BL
                            ScheduleStatus = updatedOrder.ScheduleStatus, // Updated from BL
                            TimeLeft = updatedOrder.TimeLeft, // Updated from BL
                            DeliveryList = updatedOrder.DeliveryList // Updated from BL
                        };
                    }
                    catch (Exception)
                    {
                        // Order was deleted or no longer accessible
                        Close();
                    }
                });
            }
            finally
            {
                _observerMutex.UnsetInProgress();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (CurrentOrder != null && CurrentOrder.Id != 0)
            {
                (s_bl.Order as IObservable)?.AddObserver(OrderObserver);
                _observerRegistered = true;
            }
        }

        private void Window_Closed(object? sender, EventArgs e)
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_disposed) return;

            if (_observerRegistered && CurrentOrder != null && CurrentOrder.Id != 0)
            {
                try
                {
                    (s_bl.Order as IObservable)?.RemoveObserver(OrderObserver);
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

        ~OrderWindow()
        {
            Dispose();
        }

        private BO.Order CloneOrder(BO.Order src)
        {
            if (src == null)
            {
                return new BO.Order()
                {
                    Id = 0,
                    Type = OrderType.Burger,
                    CustomerAddress = string.Empty,
                    CustomerName = string.Empty,
                    CustomerPhone = string.Empty,
                    OrderDate = s_bl.Admin.GetClock()
                };
            }

            return new BO.Order()
            {
                Id = src.Id,
                Type = src.Type,
                Description = src.Description,
                CustomerAddress = src.CustomerAddress,
                Latitude = src.Latitude,
                Longitude = src.Longitude,
                Distance = src.Distance,
                CustomerName = src.CustomerName,
                CustomerPhone = src.CustomerPhone,
                OrderDate = src.OrderDate,
                ExpectedDelivery = src.ExpectedDelivery,
                MaxDelivery = src.MaxDelivery,
                Status = src.Status,
                ScheduleStatus = src.ScheduleStatus,
                TimeLeft = src.TimeLeft,
                DeliveryList = src.DeliveryList
            };
        }
    }
}
