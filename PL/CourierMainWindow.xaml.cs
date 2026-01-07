using System.Linq;
using System.Windows;

namespace PL
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

            try
            {
                int adminId = s_bl.Admin.GetConfig().AdminId;
                _userId = adminId;

                var firstCourier = s_bl.Courier.GetList(adminId).FirstOrDefault();
                if (firstCourier != null)
                {
                    _courierId = firstCourier.Id;
                    LoadCourierData();
                }
            }
            catch
            {
                // ignore
            }
        }

        public CourierMainWindow(int userId, int courierId)
        {
            InitializeComponent();
            
            _userId = userId;
            _courierId = courierId;
            LoadCourierData();
        }

        private void LoadCourierData()
        {
            try
            {
                CurrentCourier = s_bl.Courier.Get(_userId, _courierId);
                CurrentOrderInProgress = CurrentCourier?.OrderInProgress;
            }
            catch
            {
                CurrentCourier = null;
                CurrentOrderInProgress = null;
            }
        }

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new Order.OrderListWindow();
                win.Show();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error opening history: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new Order.OrderListWindow();
                win.Show();

                this.Activated += (s, args) =>
                {
                    LoadCourierData();
                };
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error opening orders: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
