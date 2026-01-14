using BlApi;
using BO;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.Win32;

namespace PL.Courier
{
    public partial class OpenOrdersForCourierWindow : Window, INotifyPropertyChanged
    {
        private readonly IBl _bl = Factory.Get();
        private readonly int _adminId;
        private readonly int _courierId;

        /// <summary>
        /// Observer mutex to prevent concurrent observer callbacks
        /// </summary>
        private readonly ObserverMutex _observerMutex = new();

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
                    _ = LoadDataAsync();
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
                    _ = LoadDataAsync();
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

        private System.Collections.Generic.IEnumerable<OrderTypeOption> _orderTypeOptions = Array.Empty<OrderTypeOption>();
        public System.Collections.Generic.IEnumerable<OrderTypeOption> OrderTypeOptions
        {
            get => _orderTypeOptions;
            private set
            {
                _orderTypeOptions = value;
                OnPropertyChanged();
            }
        }

        private System.Collections.Generic.IEnumerable<ScheduleStatusOption> _scheduleStatusOptions = Array.Empty<ScheduleStatusOption>();
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

                // Enable IE11 mode for WebBrowser control
                SetBrowserEmulationVersion();

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

                // Load data asynchronously after window loads to prevent UI freeze
                Loaded += async (s, e) => await LoadDataAsync();
            }

        private void OnOrderListUpdated()
        {
            // Check if already processing - if so, exit immediately
            if (_observerMutex.CheckAndSetInProgress())
                return;

            System.Diagnostics.Debug.WriteLine($"[OpenOrdersForCourierWindow] Observer fired, refreshing order list");

            // Schedule UI update on dispatcher and properly handle mutex release
            Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    await LoadDataAsync();
                }
                finally
                {
                    _observerMutex.UnsetInProgress();
                }
            });
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[OpenOrdersForCourierWindow] Loading orders for courier {_courierId}, type filter: {SelectedType?.ToString() ?? "All"}, status filter: {SelectedStatus?.ToString() ?? "All"}");

                IEnumerable<BO.OpenOrderInList>? list = null;

                // Run BL query on background thread
                await System.Threading.Tasks.Task.Run(() =>
                {
                    list = _bl.Order.GetOpenOrdersForCourier(_adminId, _courierId, SelectedType, null);

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
                });

                // Update UI on UI thread
                Orders.Clear();
                foreach (var o in list!)
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

                    // Use Dispatcher.InvokeAsync to ensure UI updates happen on the UI thread
                    await Dispatcher.InvokeAsync(() =>
                    {
                        ModernMessageBox.Show($"Order {orderId} assigned.", "Success", ModernMessageBox.MessageBoxType.Success, ModernMessageBox.MessageBoxButtons.OK, this);
                        DialogResult = true;
                        Close();
                    });
                }
            }
            catch (Exception ex)
            {
                // Use Dispatcher.InvokeAsync for error handling as well
                await Dispatcher.InvokeAsync(() =>
                {
                    ModernMessageBox.Show($"Unable to assign: {ex.Message}", "Error", ModernMessageBox.MessageBoxType.Error, ModernMessageBox.MessageBoxButtons.OK, this);

                    // Re-enable the button if there was an error
                    if (sender is System.Windows.Controls.Button btn)
                    {
                        btn.IsEnabled = true;
                    }
                });
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

        private void SetBrowserEmulationVersion()
        {
            try
            {
                var appName = System.IO.Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "");
                var featureControl = @"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION";

                using (var key = Registry.CurrentUser.CreateSubKey(featureControl, true))
                {
                    if (key != null)
                    {
                        // 11001 = IE11 edge mode
                        key.SetValue(appName, 11001, RegistryValueKind.DWord);
                        System.Diagnostics.Debug.WriteLine($"[OpenOrdersForCourierWindow] Set browser emulation to IE11 for {appName}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[OpenOrdersForCourierWindow] Could not set browser emulation: {ex.Message}");
            }
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

                        // Update the map with the selected order location
                        ShowLeafletMap(compLat, compLon, ordLat, ordLon, routeDistanceKm);
                    }

                        private void ShowLeafletMap(double compLat, double compLon, double ordLat, double ordLon, double routeDistanceKm)
                        {
                            string vehicleColor = _vehicle switch
                            {
                                BO.Vehicle.Car => "#2E86DE",
                                BO.Vehicle.Motorcycle => "#8E44AD",
                                BO.Vehicle.Bicycle => "#27AE60",
                                BO.Vehicle.Foot => "#E67E22",
                                _ => "#2E86DE"
                            };

                            string vehicleIcon = _vehicle switch
                            {
                                BO.Vehicle.Car => "🚗",
                                BO.Vehicle.Motorcycle => "🏍️",
                                BO.Vehicle.Bicycle => "🚴",
                                BO.Vehicle.Foot => "🚶",
                                _ => "📍"
                            };

                                                                string html = $@"<!DOCTYPE html>
                                    <html>
                                    <head>
                                        <meta http-equiv='X-UA-Compatible' content='IE=11' />
                                        <meta charset='utf-8' />
                                        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                                        <title>Order Map</title>
                                        <link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css' 
                                              integrity='sha256-p4NxAoJBhIIN+hmNHrzRCf9tD/miZyoHS5obTRR9BMY='
                                              crossorigin='' />
                                        <script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'
                                                integrity='sha256-20nQCchB9co0qIjJZRGuk2/Z9VM+kNiyxNV1lvTlZBo='
                                                crossorigin=''></script>
                                        <style>
                                            html, body {{
                                                height: 100%;
                                                margin: 0;
                                                padding: 0;
                                            }}
                                            #map {{
                                                width: 100%;
                                                height: 100%;
                                            }}
                                        </style>
                                    </head>
                                    <body>
                                        <div id='map'></div>
                                        <script>
                                            var map = L.map('map');

                                            // Add OpenStreetMap tile layer
                                            L.tileLayer('https://{{s}}.tile.openstreetmap.org/{{z}}/{{x}}/{{y}}.png', {{
                                                attribution: '© OpenStreetMap contributors',
                                                maxZoom: 19
                                            }}).addTo(map);

                                            // Company marker (green)
                                            var companyMarker = L.marker([{compLat.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {compLon.ToString(System.Globalization.CultureInfo.InvariantCulture)}], {{
                                                icon: L.divIcon({{
                                                    html: '<div style=""background-color: #28a745; width: 24px; height: 24px; border-radius: 50%; border: 3px solid white; box-shadow: 0 2px 5px rgba(0,0,0,0.3);""></div>',
                                                    className: 'custom-marker',
                                                    iconSize: [24, 24],
                                                    iconAnchor: [12, 12]
                                                }})
                                            }}).addTo(map);
                                            companyMarker.bindPopup('<b>Company Location</b><br>Start Point');

                                            // Order marker (red)
                                            var orderMarker = L.marker([{ordLat.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {ordLon.ToString(System.Globalization.CultureInfo.InvariantCulture)}], {{
                                                icon: L.divIcon({{
                                                    html: '<div style=""background-color: #dc3545; width: 24px; height: 24px; border-radius: 50%; border: 3px solid white; box-shadow: 0 2px 5px rgba(0,0,0,0.3);""></div>',
                                                    className: 'custom-marker',
                                                    iconSize: [24, 24],
                                                    iconAnchor: [12, 12]
                                                }})
                                            }}).addTo(map);
                                            orderMarker.bindPopup('<b>Order Destination</b><br>Delivery Point');

                                            // Draw straight line between company and order location
                                            var straightLine = L.polyline([
                                                [{compLat.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {compLon.ToString(System.Globalization.CultureInfo.InvariantCulture)}],
                                                [{ordLat.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {ordLon.ToString(System.Globalization.CultureInfo.InvariantCulture)}]
                                            ], {{
                                                color: '{vehicleColor}',
                                                weight: 4,
                                                opacity: 0.7,
                                                dashArray: '10, 10'
                                            }}).addTo(map);

                                            // Fit bounds to show all markers
                                            var bounds = L.latLngBounds([
                                                [{compLat.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {compLon.ToString(System.Globalization.CultureInfo.InvariantCulture)}],
                                                [{ordLat.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {ordLon.ToString(System.Globalization.CultureInfo.InvariantCulture)}]
                                            ]);
                                            map.fitBounds(bounds, {{ padding: [50, 50] }});

                                            // Add info panel
                                            var info = L.control({{ position: 'bottomright' }});
                                            info.onAdd = function(map) {{
                                                var div = L.DomUtil.create('div', 'info-panel');
                                                div.style.background = 'white';
                                                div.style.padding = '10px';
                                                div.style.borderRadius = '8px';
                                                div.style.boxShadow = '0 2px 10px rgba(0,0,0,0.2)';
                                                div.style.fontSize = '12px';
                                                div.innerHTML = '<b>{vehicleIcon} Vehicle: {_vehicle}</b><br>' +
                                                               'Distance: <b>{routeDistanceKm.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)} km</b>';
                                                return div;
                                            }};
                                            info.addTo(map);
                                        </script>
                                    </body>
                                    </html>";

                                                                wbMap.NavigateToString(html);
                                                            }
                }
            }
