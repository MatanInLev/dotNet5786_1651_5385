using BlApi;
using BO;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace PL.Courier
{
    public partial class OpenOrdersForCourierWindow : Window, INotifyPropertyChanged
    {
        private readonly IBl _bl = Factory.Get();
        private readonly int _adminId;
        private readonly int _courierId;

        public ObservableCollection<OpenOrderInList> Orders { get; } = new();

        public OrderType? SelectedType { get; set; } = null;

        private OpenOrderInList? _selectedOrder;
        public OpenOrderInList? SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                if (_selectedOrder != value)
                {
                    _selectedOrder = value;
                    OnPropertyChanged();
                }
            }
        }

        public record OrderTypeOption(OrderType? Value, string Label);

        public System.Collections.Generic.IEnumerable<OrderTypeOption> OrderTypeOptions { get; private set; }

        private readonly BO.Vehicle _vehicle;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public OpenOrdersForCourierWindow(int adminId, int courierId)
        {
            _adminId = adminId;
            _courierId = courierId;
            _vehicle = _bl.Courier.Get(_adminId, _courierId).Vehicle;
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
                ModernMessageBox.Show($"Error loading: {ex.Message}", "Error", ModernMessageBox.MessageBoxType.Error, ModernMessageBox.MessageBoxButtons.OK, this);
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
                    ModernMessageBox.Show($"Order {orderId} assigned.", "Success", ModernMessageBox.MessageBoxType.Success, ModernMessageBox.MessageBoxButtons.OK, this);
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                ModernMessageBox.Show($"Unable to assign: {ex.Message}", "Error", ModernMessageBox.MessageBoxType.Error, ModernMessageBox.MessageBoxButtons.OK, this);
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
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            (_bl.Order as IObservable)?.RemoveObserver(LoadData);
            base.OnClosed(e);
        }

        private void ShowMapForSelection(OpenOrderInList sel)
        {
            if (sel == null) return;

            double compLat = sel.CompanyLatitude;
            double compLon = sel.CompanyLongitude;
            double ordLat = sel.Latitude;
            double ordLon = sel.Longitude;

            double routeDistanceKm = sel.DistanceFromCompany;
            try
            {
                routeDistanceKm = BO.Tools.CalculateRouteDistance(compLat, compLon, ordLat, ordLon, _vehicle);
            }
            catch
            {
                // keep aerial if routing fails
            }

            // Simple simulated route polyline between points to visualize driving/walking/bike
            // Build a couple of intermediate points to mimic a route instead of a straight line
            double midLat = (compLat + ordLat) / 2.0 + 0.005;
            double midLon = (compLon + ordLon) / 2.0 - 0.005;

            string vehicleColor = _vehicle switch
            {
                BO.Vehicle.Car => "#2E86DE",
                BO.Vehicle.Motorcycle => "#8E44AD",
                BO.Vehicle.Bicycle => "#27AE60",
                BO.Vehicle.Foot => "#E67E22",
                _ => "#2E86DE"
            };

            string summary = $"Company: {compLat:F4},{compLon:F4} | Order: {ordLat:F4},{ordLon:F4} | Vehicle: {_vehicle} | Route: {routeDistanceKm:F2} km";

            string html = $@"<html>
  <head>
    <meta http-equiv='X-UA-Compatible' content='IE=Edge'/>
    <style>
      html, body {{
        height: 100%;
        margin: 0;
        padding: 0;
        overflow: hidden;
        font-family: Segoe UI, Arial;
      }}
      .container {{
        display: flex;
        flex-direction: column;
        height: 100%;
        padding: 10px;
        box-sizing: border-box;
      }}
      h4 {{
        margin: 0 0 8px 0;
        font-size: 60px;
        color: #2C3E50;
      }}
      .label {{
        font-size: 50px;
        color: #495057;
        margin-bottom: 10px;
      }}
      .map-container {{
        flex: 1;
        display: flex;
        align-items: center;
        justify-content: center;
        min-height: 0;
      }}
      svg {{
        width: 100%;
        height: 100%;
        max-width: 100%;
        max-height: 100%;
      }}
    </style>
  </head>
  <body>
    <div class='container'>
      <h4>Map (demo)</h4>
      <div class='label'>{summary}</div>
      <div class='map-container'>
        <svg viewBox='0 0 900 260' preserveAspectRatio='xMidYMid meet' xmlns='http://www.w3.org/2000/svg'>
          <!-- Aerial line -->
          <line x1='80' y1='180' x2='820' y2='60' stroke='gray' stroke-width='1.5' stroke-dasharray='4'/>
          <!-- Simulated route polyline -->
          <polyline points='80,180 450,200 820,60' fill='none' stroke='{vehicleColor}' stroke-width='3' stroke-linejoin='round' stroke-linecap='round' />
          <circle cx='80' cy='180' r='7' fill='green' />
          <text x='85' y='165' font-size='30'>Company ({compLat:F4},{compLon:F4})</text>
          <circle cx='820' cy='60' r='7' fill='red' />
          <text x='640' y='45' font-size='30'>Order ({ordLat:F4},{ordLon:F4})</text>
          <text x='360' y='225' font-size='30'>Aerial distance: {sel.DistanceFromCompany:F2} km</text>
          <text x='360' y='255' font-size='30'>Route distance: {routeDistanceKm:F2} km</text>
          <text x='360' y='285' font-size='30'>(mode: {_vehicle})</text>
        </svg>
      </div>
    </div>
  </body>
</html>
";

            wbMap.NavigateToString(html);
        }
    }
}
