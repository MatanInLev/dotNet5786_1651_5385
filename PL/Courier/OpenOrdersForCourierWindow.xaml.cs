using BlApi;
using BO;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace PL.Courier
{
    public partial class OpenOrdersForCourierWindow : Window
    {
        private readonly IBl _bl = Factory.Get();
        private readonly int _adminId;
        private readonly int _courierId;

        public ObservableCollection<OpenOrderInList> Orders { get; } = new();

        public OrderType? SelectedType { get; set; } = null;

        public OpenOrderInList? SelectedOrder { get; set; }

        public record OrderTypeOption(OrderType? Value, string Label);

        public System.Collections.Generic.IEnumerable<OrderTypeOption> OrderTypeOptions { get; private set; }

        public OpenOrdersForCourierWindow(int adminId, int courierId)
        {
            _adminId = adminId;
            _courierId = courierId;
            InitializeComponent();
            OrderTypeOptions = Enum.GetValues(typeof(OrderType))
                                    .Cast<OrderType>()
                                    .Select(t => new OrderTypeOption(t, t.ToString()))
                                    .Prepend(new OrderTypeOption(null, "All"))
                                    .ToList();
            DataContext = this;
            LoadData();
            (_bl.Order as IObservable)?.AddObserver(LoadData);
        }

        private void LoadData()
        {
            try
            {
                Orders.Clear();
                var list = _bl.Order.GetOpenOrdersForCourier(_adminId, _courierId, SelectedType, null);
                foreach (var o in list)
                    Orders.Add(o);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void Collect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is FrameworkElement fe && fe.Tag is int orderId)
                {
                    _bl.Order.AssignOrder(_adminId, orderId, _courierId);
                    MessageBox.Show($"Order {orderId} assigned.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to assign: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Orders_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (sender is System.Windows.Controls.DataGrid dg)
            {
                SelectedOrder = dg.SelectedItem as OpenOrderInList;
                ShowMapForSelection(SelectedOrder);
                DataContext = null;
                DataContext = this;
            }
        }

        private void ShowMapForSelection(OpenOrderInList sel)
        {
            if (sel == null) return;

            double compLat = sel.CompanyLatitude;
            double compLon = sel.CompanyLongitude;
            double ordLat = sel.Latitude;
            double ordLon = sel.Longitude;

            string summary = $"Company: {compLat:F4},{compLon:F4} | Order: {ordLat:F4},{ordLon:F4}";

            string html = $@"<html>
  <head>
    <meta http-equiv='X-UA-Compatible' content='IE=Edge'/>
    <style>body {{ font-family: Segoe UI, Arial; }} .label{{font-size:12px;}}</style>
  </head>
  <body>
    <h4>Map (demo)</h4>
    <div class='label'>{summary}</div>
    <div>
      <svg width='900' height='220' xmlns='http://www.w3.org/2000/svg'>
        <line x1='50' y1='110' x2='850' y2='110' stroke='blue' stroke-width='2' stroke-dasharray='6'/>
        <circle cx='50' cy='110' r='6' fill='green' />
        <text x='70' y='105' class='label'>Company ({compLat:F4},{compLon:F4})</text>
        <circle cx='850' cy='110' r='6' fill='red' />
        <text x='600' y='105' class='label'>Order ({ordLat:F4},{ordLon:F4})</text>
        <text x='400' y='140' class='label'>Aerial distance: {sel.DistanceFromCompany:F2} km</text>
      </svg>
    </div>
  </body>
</html>
";

            wbMap.NavigateToString(html);
        }

        protected override void OnClosed(EventArgs e)
        {
            (_bl.Order as IObservable)?.RemoveObserver(LoadData);
            base.OnClosed(e);
        }
    }
}
