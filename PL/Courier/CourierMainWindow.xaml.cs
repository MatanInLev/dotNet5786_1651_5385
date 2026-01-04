using BlApi;
using BO;
using System;
using System.Windows;

namespace PL.Courier
{
    public partial class CourierMainWindow : Window
    {
        static readonly IBl s_bl = Factory.Get();

        private int _adminId;
        private int _courierId;

        public BO.Courier? CurrentCourier
        {
            get => (BO.Courier?)GetValue(CurrentCourierProperty);
            set => SetValue(CurrentCourierProperty, value);
        }

        public static readonly DependencyProperty CurrentCourierProperty =
            DependencyProperty.Register(nameof(CurrentCourier), typeof(BO.Courier), typeof(CourierMainWindow), new PropertyMetadata(null));

        public object? CurrentOrderInProgress
        {
            get => GetValue(CurrentOrderInProgressProperty);
            set => SetValue(CurrentOrderInProgressProperty, value);
        }

        public static readonly DependencyProperty CurrentOrderInProgressProperty =
            DependencyProperty.Register(nameof(CurrentOrderInProgress), typeof(object), typeof(CourierMainWindow), new PropertyMetadata(null));

        public CourierMainWindow()
        {
            InitializeComponent();
        }

        public CourierMainWindow(int adminId, int courierId) : this()
        {
            _adminId = adminId;
            _courierId = courierId;

            try
            {
                CurrentCourier = s_bl.Courier.Get(_adminId, _courierId);
            }
            catch
            {
                CurrentCourier = null;
            }

            // Optionally load current order in progress (left null if none)
            try
            {
                // This BL may not have a direct call; leaving null is acceptable for now
                CurrentOrderInProgress = null;
            }
            catch
            {
                CurrentOrderInProgress = null;
            }
        }

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new ClosedDeliveryListWindow(_adminId, _courierId) { Owner = this };
                win.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening history: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Open OrderListWindow (courier's view of orders may differ)
                new PL.Order.OrderListWindow().Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening orders: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
