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

        private OrderType? _selectedType = null;
        public OrderType? SelectedType
        {
            get => _selectedType;
            set
            {
                if (_selectedType != value)
                {
                    _selectedType = value;
                    OnPropertyChanged();
                    LoadData();
                }
            }
        }

        private ScheduleStatus? _selectedStatus = null;
        public ScheduleStatus? SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                if (_selectedStatus != value)
                {
                    _selectedStatus = value;
                    OnPropertyChanged();
                    LoadData();
                }
            }
        }

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
        public record ScheduleStatusOption(ScheduleStatus? Value, string Label);

        private System.Collections.Generic.IEnumerable<OrderTypeOption> _orderTypeOptions;
        public System.Collections.Generic.IEnumerable<OrderTypeOption> OrderTypeOptions
        {
            get => _orderTypeOptions;
            private set
            {
                _orderTypeOptions = value;
                OnPropertyChanged();
            }
        }

        private System.Collections.Generic.IEnumerable<ScheduleStatusOption> _scheduleStatusOptions;
        public System.Collections.Generic.IEnumerable<ScheduleStatusOption> ScheduleStatusOptions
        {
            get => _scheduleStatusOptions;
            private set
            {
                _scheduleStatusOptions = value;
                OnPropertyChanged();
            }
        }

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
            
            // Create filter options with "All" as the first option BEFORE InitializeComponent
            OrderTypeOptions = new[]
            {
                new OrderTypeOption(null, "All")
            }.Concat(
                Enum.GetValues(typeof(OrderType))
                    .Cast<OrderType>()
                    .Select(t => new OrderTypeOption(t, t.ToString()))
            ).ToList();

            ScheduleStatusOptions = new[]
            {
                new ScheduleStatusOption(null, "All")
            }.Concat(
                Enum.GetValues(typeof(ScheduleStatus))
                    .Cast<ScheduleStatus>()
                    .Select(s => new ScheduleStatusOption(s, s.ToString()))
            ).ToList();
            
            InitializeComponent();
            
            // Set default selection to "All" (null value)
            SelectedType = null;
            SelectedStatus = null;
            
            System.Diagnostics.Debug.WriteLine($"[OpenOrdersForCourierWindow] Registering observer for order list");
            (_bl.Order as IObservable)?.AddObserver(OnOrderListUpdated);
            
            LoadData();
        }
        
        private void OnOrderListUpdated()
        {
            System.Diagnostics.Debug.WriteLine($"[OpenOrdersForCourierWindow] Observer fired, refreshing order list");
            Dispatcher.Invoke(() => LoadData());
        }

        private void LoadData()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[OpenOrdersForCourierWindow] Loading orders for courier {_courierId}, type filter: {SelectedType?.ToString() ?? "All"}, status filter: {SelectedStatus?.ToString() ?? "All"}");
                Orders.Clear();
                var list = _bl.Order.GetOpenOrdersForCourier(_adminId, _courierId, SelectedType, null);
                
                // Apply client-side ScheduleStatus filter if selected
                if (SelectedStatus.HasValue)
                {
                    list = list.Where(o => o.ScheduleStatus == SelectedStatus.Value);
                }
                
                // Sort by priority: Risk first, then Late, then OnTime
                list = list.OrderBy(o => o.ScheduleStatus switch
                {
                    ScheduleStatus.Risk => 0,
                    ScheduleStatus.Late => 1,
                    ScheduleStatus.OnTime => 2,
                    _ => 3
                });
                
                foreach (var o in list)
                    Orders.Add(o);
                System.Diagnostics.Debug.WriteLine($"[OpenOrdersForCourierWindow] Loaded {Orders.Count} orders");
            }
            catch (Exception ex)
            {
                ModernMessageBox.Show($"Error loading: {ex.Message}", "Error", ModernMessageBox.MessageBoxType.Error, ModernMessageBox.MessageBoxButtons.OK, this);
            }
        }

        private async void Collect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is FrameworkElement fe && fe.Tag is int orderId)
                {
                    // Disable the button to prevent multiple clicks
                    if (sender is System.Windows.Controls.Button btn)
                    {
                        btn.IsEnabled = false;
                    }

                    // Run the assignment on a background thread to avoid UI freeze
                    await System.Threading.Tasks.Task.Run(() =>
                    {
                        _bl.Order.AssignOrder(_adminId, orderId, _courierId);
                    });

                    ModernMessageBox.Show($"Order {orderId} assigned.", "Success", ModernMessageBox.MessageBoxType.Success, ModernMessageBox.MessageBoxButtons.OK, this);
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                ModernMessageBox.Show($"Unable to assign: {ex.Message}", "Error", ModernMessageBox.MessageBoxType.Error, ModernMessageBox.MessageBoxButtons.OK, this);
                
                // Re-enable the button if there was an error
                if (sender is System.Windows.Controls.Button btn)
                {
                    btn.IsEnabled = true;
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        
        private void ClearFilter_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[OpenOrdersForCourierWindow] Clearing filters");
            SelectedType = null;
            SelectedStatus = null;
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
            System.Diagnostics.Debug.WriteLine($"[OpenOrdersForCourierWindow] Removing observer");
            (_bl.Order as IObservable)?.RemoveObserver(OnOrderListUpdated);
            base.OnClosed(e);
        }

        private void ShowMapForSelection(OpenOrderInList? sel)
        {
            if (sel == null) return;

            double compLat = sel.CompanyLatitude;
            double compLon = sel.CompanyLongitude;
            double ordLat = sel.Latitude;
            double ordLon = sel.Longitude;

            double routeDistanceKm = sel.DistanceFromCompany;
            
            // Use cached route distance if available, otherwise use aerial distance
            // Don't make network calls on UI thread
            var cacheKey = (
                Math.Round(compLat, 4),
                Math.Round(compLon, 4),
                Math.Round(ordLat, 4),
                Math.Round(ordLon, 4),
                _vehicle
            );
            
            if (BO.Tools.TryGetCachedRouteDistance(cacheKey.Item1, cacheKey.Item2, cacheKey.Item3, cacheKey.Item4, cacheKey.Item5, out double cached))
            {
                routeDistanceKm = cached;
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
