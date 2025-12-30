using System.Windows;

namespace PL.Courier
{
    public partial class CourierMainWindow : Window
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        private readonly int _userId;
        private readonly int _courierId;

        public BO.Courier? CurrentCourier
        {
            get => (BO.Courier?)GetValue(CurrentCourierProperty);
            set => SetValue(CurrentCourierProperty, value);
        }

        public static readonly DependencyProperty CurrentCourierProperty =
            DependencyProperty.Register(
                nameof(CurrentCourier),
                typeof(BO.Courier),
                typeof(CourierMainWindow),
                new PropertyMetadata(null));

        public CourierMainWindow()
        {
            try
            {
                InitializeComponent();

                int adminId = s_bl.Admin.GetConfig().AdminId;
                _userId = adminId;

                var firstCourier = s_bl.Courier
                    .GetList(adminId)
                    .FirstOrDefault();

                if (firstCourier == null)
                {
                    MessageBox.Show("No courier exists in the system yet.",
                                    "Info",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                    return;
                }

                _courierId = firstCourier.Id;
                CurrentCourier = s_bl.Courier.Get(_userId, _courierId);

                DataContext = this;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading courier data: {ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        public CourierMainWindow(int userId, int courierId)
        {
            try
            {
                InitializeComponent();

                _userId = userId;
                _courierId = courierId;

                CurrentCourier = s_bl.Courier.Get(_userId, _courierId);

                DataContext = this;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading courier data: {ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void BtnFinishHandling_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Finish handling clicked (BL call to implement).",
                            "Info",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }

        private void BtnHandlingDetails_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Handling details clicked (open details window).",
                            "Info",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }

        private void BtnMarkCompleted_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentCourier?.OrderInProgress == null)
            {
                MessageBox.Show("There is no order in progress.",
                                "Info",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                return;
            }

            MessageBox.Show("Mark as delivery completed clicked (call BL here).",
                            "Info",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Open delivery history screen (to implement).",
                            "Info",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }

        private void BtnOrderSelection_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Open order selection screen (to implement).",
                            "Info",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }
    }
}
