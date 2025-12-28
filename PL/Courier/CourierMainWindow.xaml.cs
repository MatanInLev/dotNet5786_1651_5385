using System.Windows;

namespace PL.Courier;
public partial class CourierMainWindow : Window
{
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

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

    public CourierMainWindow(int courierId = 0)
    {
        InitializeComponent();
        DataContext = this;

        if (courierId != 0)
        {
            try
            {
                int adminId = s_bl.Admin.GetConfig().AdminId;
                CurrentCourier = s_bl.Courier.Get(adminId, courierId);
                CurrentOrderInProgress = null; // Explicitly set to null, no method to fetch order in progress
            }
            catch
            {
                CurrentCourier = null;
                CurrentOrderInProgress = null;
            }
        }
    }

    private void BtnUpdateCourier_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentCourier == null)
        {
            new CourierWindow().Show();
            return;
        }
        new CourierWindow(CurrentCourier.Id).Show(); 
    }

    private void BtnSelectOrder_Click(object sender, RoutedEventArgs e)
    {
        new CourierListWindow().Show();
    }

    private void BtnHistory_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Open delivery history (not implemented)", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void BtnFinishHandling_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Finish handling clicked", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void BtnHandlingDetails_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Handling details clicked", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void BtnMarkCompleted_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Mark as delivery completed clicked", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
