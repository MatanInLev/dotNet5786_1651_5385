using BlApi;
using BO;
using System;
using System.Collections.Generic;
using System.Windows;

namespace PL.Courier
{
    public partial class ClosedDeliveryListWindow : Window
    {
        static readonly IBl s_bl = Factory.Get();

        public IEnumerable<ClosedDeliveryInList> ClosedDeliveries
        {
            get => (IEnumerable<ClosedDeliveryInList>)GetValue(ClosedDeliveriesProperty);
            set => SetValue(ClosedDeliveriesProperty, value);
        }

        public static readonly DependencyProperty ClosedDeliveriesProperty =
            DependencyProperty.Register(nameof(ClosedDeliveries), typeof(IEnumerable<ClosedDeliveryInList>), typeof(ClosedDeliveryListWindow), new PropertyMetadata(null));

        private int _userId;
        private int _courierId;

        public ClosedDeliveryListWindow(int userId, int courierId)
        {
            InitializeComponent();
            _userId = userId;
            _courierId = courierId;

            // load default
            QueryList();

            // subscribe to updates
            (s_bl.Order as IObservable)?.AddObserver(QueryList);
        }

        private void QueryList()
        {
            try
            {
                // read filters
                BO.OrderType? typeFilter = null;
                var typeItem = cmbType.SelectedItem;
                if (typeItem is System.Windows.Controls.ComboBoxItem cb && cb.Content?.ToString() != "All")
                {
                    if (Enum.TryParse(cb.Content?.ToString(), out BO.OrderType parsed)) typeFilter = parsed;
                }

                BO.DeliveryStatus? statusFilter = null;
                var statusItem = cmbEndStatus.SelectedItem as System.Windows.Controls.ComboBoxItem;
                if (statusItem != null && statusItem.Content?.ToString() != "All")
                {
                    if (Enum.TryParse(statusItem.Content?.ToString(), out BO.DeliveryStatus st)) statusFilter = st;
                }

                string? sortTag = null;
                if (cmbSort.SelectedItem is System.Windows.Controls.ComboBoxItem sortItem)
                    sortTag = sortItem.Tag as string;

                var list = s_bl.Order.GetClosedOrdersForCourier(_userId, _courierId, typeFilter, null);

                // apply status filter client-side
                if (statusFilter != null) list = System.Linq.Enumerable.Where(list, d => d.DeliveryEndStatus == statusFilter);

                // apply sorting client-side
                list = sortTag switch
                {
                    "processing" => System.Linq.Enumerable.OrderBy(list, d => d.ProcessingTime),
                    "distance" => System.Linq.Enumerable.OrderBy(list, d => d.ActualDistance ?? double.MaxValue),
                    "status" => System.Linq.Enumerable.OrderBy(list, d => d.DeliveryEndStatus),
                    _ => System.Linq.Enumerable.OrderBy(list, d => d.DeliveryEndStatus) // default
                };

                ClosedDeliveries = list;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading closed deliveries: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Filter_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            QueryList();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            QueryList();
        }

        protected override void OnClosed(EventArgs e)
        {
            (s_bl.Order as IObservable)?.RemoveObserver(QueryList);
            base.OnClosed(e);
        }
    }
}
