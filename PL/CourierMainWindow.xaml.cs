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
        }

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Open delivery history (not implemented)", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnOrderSelection_Click(object sender, RoutedEventArgs e)
        {
            // Open OrderListWindow (selection of orders) as required by the spec
            var ordersWin = new Order.OrderListWindow();
            ordersWin.Show();
        }
    }
}
