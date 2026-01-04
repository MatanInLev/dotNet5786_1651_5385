using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PL.Courier
{
    public partial class CourierMainWindow : Window
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        private int _userId = 0;
        private int _courierId = 0;

        public BO.Courier? CurrentCourier
        {
            get => (BO.Courier?)GetValue(CurrentCourierProperty);
            set => SetValue(CurrentCourierProperty, value);
        }

        public static readonly DependencyProperty CurrentCourierProperty =
            DependencyProperty.Register(nameof(CurrentCourier), typeof(BO.Courier), typeof(CourierMainWindow), new PropertyMetadata(null));

        public BO.OrderInProgress? CurrentOrderInProgress
        {
            get => (BO.OrderInProgress?)GetValue(CurrentOrderInProgressProperty);
            set => SetValue(CurrentOrderInProgressProperty, value);
        }

        public static readonly DependencyProperty CurrentOrderInProgressProperty =
            DependencyProperty.Register(nameof(CurrentOrderInProgress), typeof(BO.OrderInProgress), typeof(CourierMainWindow), new PropertyMetadata(null));

        public CourierMainWindow()
        {
            InitializeComponent();
            DataContext = this;

            try
            {
                int adminId = s_bl.Admin.GetConfig().AdminId;
                _userId = adminId;

                var firstCourier = s_bl.Courier.GetList(adminId).FirstOrDefault();
                if (firstCourier != null)
                {
                    _courierId = firstCourier.Id;
                    CurrentCourier = s_bl.Courier.Get(_userId, _courierId);
                }
            }
            catch
            {
                // ignore
            }

            UpdatePanels();
        }

        public CourierMainWindow(int userId, int courierId)
            : this()
        {
            _userId = userId;
            _courierId = courierId;

            try
            {
                CurrentCourier = s_bl.Courier.Get(_userId, _courierId);
            }
            catch
            {
                CurrentCourier = null;
            }

            UpdatePanels();
        }

        private void UpdatePanels()
        {
            // If courier or UI not ready, skip
            if (!IsLoaded || CurrentCourier == null) return;

            var orderPanel = this.FindName("orderPanel") as StackPanel;
            var noOrderPanel = this.FindName("noOrderPanel") as StackPanel;
            var btnOrderSelection = this.FindName("btnOrderSelection") as Button;
            var cmbVehicleMain = this.FindName("cmbVehicleMain") as ComboBox;

            // Show order panel if there is an order in progress
            if (CurrentCourier.OrderInProgress != null)
            {
                if (orderPanel != null) orderPanel.Visibility = Visibility.Visible;
                if (noOrderPanel != null) noOrderPanel.Visibility = Visibility.Collapsed;
                if (btnOrderSelection != null) btnOrderSelection.IsEnabled = false;
                if (cmbVehicleMain != null) cmbVehicleMain.IsEnabled = false; // cannot change vehicle during active delivery
            }
            else
            {
                if (orderPanel != null) orderPanel.Visibility = Visibility.Collapsed;
                if (noOrderPanel != null) noOrderPanel.Visibility = Visibility.Visible;
                // If courier not active, cannot choose orders
                if (btnOrderSelection != null) btnOrderSelection.IsEnabled = CurrentCourier.IsActive;
                if (cmbVehicleMain != null) cmbVehicleMain.IsEnabled = true;
            }
        }

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            // Open closed deliveries history for this courier
            try
            {
                int adminId = s_bl.Admin.GetConfig().AdminId;
                var win = new PL.Order.OrderListWindow();
                win.Show();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error opening history: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnOrderSelection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Open order selection filtered to this courier
                var win = new OrderSelectionWindow(_userId, _courierId) { Owner = this };
                win.ShowDialog();

                // After dialog closes, refresh courier (maybe assigned)
                if (CurrentCourier != null)
                {
                    CurrentCourier = s_bl.Courier.Get(_userId, CurrentCourier.Id);
                    UpdatePanels();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error opening order selection: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentCourier == null) return;

                int adminId = s_bl.Admin.GetConfig().AdminId;

                // Validate max distance against company max (BL rules say it must be <= company max)
                var companyMax = s_bl.Admin.GetConfig().MaxRange; // use MaxRange from config
                if (CurrentCourier.MaxDistance.HasValue && companyMax > 0 && CurrentCourier.MaxDistance.Value > companyMax)
                {
                    MessageBox.Show($"Max distance cannot exceed company maximum of {companyMax} km.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                s_bl.Courier.Update(adminId, CurrentCourier);
                MessageBox.Show("Courier updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                UpdatePanels();
            }
            catch (BO.BlInvalidValueException ex)
            {
                MessageBox.Show($"Invalid data: {ex.Message}", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error updating courier: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnFinish_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentCourier?.OrderInProgress == null)
                {
                    MessageBox.Show("No active order to finish.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var cmbFinish = this.FindName("cmbFinishStatus") as ComboBox;
                var selectedItem = (cmbFinish?.SelectedItem as ComboBoxItem)?.Content as string;
                if (string.IsNullOrEmpty(selectedItem))
                {
                    MessageBox.Show("Please select a finish status.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                BO.DeliveryStatus status = selectedItem switch
                {
                    "Delivered" => BO.DeliveryStatus.Delivered,
                    "CustomerUnreachable" => BO.DeliveryStatus.CustomerUnreachable,
                    "Canceled" => BO.DeliveryStatus.Canceled,
                    // Fallback to Delivered if unknown
                    _ => BO.DeliveryStatus.Delivered
                };

                int adminId = s_bl.Admin.GetConfig().AdminId;
                s_bl.Order.CompleteOrderDelivery(adminId, CurrentCourier.OrderInProgress.DeliveryId, status);

                MessageBox.Show("Order finished.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Refresh courier data
                CurrentCourier = s_bl.Courier.Get(adminId, CurrentCourier.Id);
                UpdatePanels();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error finishing delivery: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
