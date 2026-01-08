using BlApi;
using BO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PL.Courier
{
    public partial class OrderSelectionWindow : Window
    {
        static readonly IBl s_bl = Factory.Get();

        private int _userId;
        private int _courierId;

        public IEnumerable<BO.OrderType> OrderTypes { get; } = Enum.GetValues(typeof(BO.OrderType)).Cast<BO.OrderType>();
        public BO.OrderType? SelectedOrderType { get; set; }

        private IEnumerable<BO.OpenOrderInList>? _orders;

        public OrderSelectionWindow(int userId, int courierId)
        {
            InitializeComponent();
            _userId = userId;
            _courierId = courierId;

            LoadOrders();
        }

        private void LoadOrders()
        {
            try
            {
                // Get open orders for courier
                _orders = s_bl.Order.GetOpenOrdersForCourier(_userId, _courierId).ToList();
                dgOrders.ItemsSource = _orders;
            }
            catch (Exception ex)
            {
                ModernMessageBox.Show($"Error loading orders: {ex.Message}", "Error", ModernMessageBox.MessageBoxType.Error, ModernMessageBox.MessageBoxButtons.OK, this);
            }
        }

        private void ApplyFilters_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<BO.OpenOrderInList> filtered = _orders ?? Array.Empty<BO.OpenOrderInList>();
            if (SelectedOrderType != null)
                filtered = filtered.Where(o => o.Type == SelectedOrderType.Value);

            // sort
            var sort = (cmbSort.SelectedItem as ComboBoxItem)?.Content as string;
            filtered = sort switch
            {
                "Distance" => filtered.OrderBy(o => o.DistanceFromCompany),
                "TimeLeft" => filtered.OrderBy(o => o.TimeLeft),
                _ => filtered.OrderBy(o => o.ScheduleStatus),
            };

            dgOrders.ItemsSource = filtered.ToList();
        }

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            SelectedOrderType = null;
            cmbTypeFilter.SelectedItem = null;
            cmbSort.SelectedIndex = 0;
            dgOrders.ItemsSource = _orders;
        }

        private void Collect_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            if (btn.Tag is not int orderId) return;

            try
            {
                int adminId = s_bl.Admin.GetConfig().AdminId;
                s_bl.Order.AssignOrder(adminId, orderId, _courierId);

                // send simple email (placeholder) - here we just simulate via MessageBox
                var orderDetail = s_bl.Order.Get(adminId, orderId);
                SendEmailToCourier(orderDetail);

                ModernMessageBox.Show("Order assigned to you. Check courier main screen.", "Assigned", ModernMessageBox.MessageBoxType.Information, ModernMessageBox.MessageBoxButtons.OK, this);

                // refresh list
                LoadOrders();
            }
            catch (Exception ex)
            {
                ModernMessageBox.Show($"Error assigning order: {ex.Message}", "Error", ModernMessageBox.MessageBoxType.Error, ModernMessageBox.MessageBoxButtons.OK, this);
            }
        }

        private void SendEmailToCourier(BO.Order order)
        {
            // Simulate email by showing message - real implementation would use SMTP
            string subject = $"Order Assignment - {order.Id}";
            string body = $"Order {order.Id}\nCustomer: {order.CustomerName}\nAddress: {order.CustomerAddress}\nType: {order.Type}\n";
            MessageBox.Show($"Email to courier:\nSubject: {subject}\n\n{body}", "Email (simulated)", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DgOrders_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dgOrders.SelectedItem is BO.OpenOrderInList sel)
            {
                // populate right panel
                tbOrderId.Text = sel.Id.ToString();
                tbCustomer.Text = sel.CourierId?.ToString() ?? "-";
                tbAddress.Text = sel.CustomerAddress;
                tbDistance.Text = sel.DistanceFromCompany.ToString("N2");
                tbSched.Text = sel.ScheduleStatus.ToString();
                tbTimeLeft.Text = sel.TimeLeft.ToString();

                // show map using simple embedded svg demo
                ShowMapForSelection(sel);
            }
        }

        private void ShowMapForSelection(BO.OpenOrderInList sel)
        {
            // BO.Config currently does not expose latitude/longitude; use placeholders for demo map.
            double compLat = 0;
            double compLon = 0;

            // Use order id to derive pseudo-coords for demo (in real app use order.Latitude/Longitude)
            double ordLat = compLat + 0.01 * ((sel.Id % 10) - 5);
            double ordLon = compLon + 0.01 * (((sel.Id / 10) % 10) - 5);

            string html = $@"
<html>
  <head>
    <meta http-equiv='X-UA-Compatible' content='IE=Edge'/>
    <style>body {{ font-family: Segoe UI, Arial; }}</style>
  </head>
  <body>
    <h4>Map (demo): company and order markers</h4>
    <div>
      <p>Company: {compLat},{compLon}</p>
      <p>Order: {ordLat},{ordLon}</p>
      <p>Distance (aerial): {sel.DistanceFromCompany:N2} km</p>
    </div>
    <div>
      <svg width='900' height='220' xmlns='http://www.w3.org/2000/svg'>
        <line x1='50' y1='110' x2='850' y2='110' stroke='blue' stroke-width='2' stroke-dasharray='6'/>
        <circle cx='50' cy='110' r='6' fill='green' />
        <text x='70' y='110' alignment-baseline='middle'>Company</text>
        <circle cx='850' cy='110' r='6' fill='red' />
        <text x='560' y='110' alignment-baseline='middle'>Order</text>
      </svg>
    </div>
  </body>
</html>
";

            wbMap.NavigateToString(html);
        }
    }
}
