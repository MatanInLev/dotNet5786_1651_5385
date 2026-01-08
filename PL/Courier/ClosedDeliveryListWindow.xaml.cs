using BlApi;
using BO;
using System;
using System.Collections.Generic;
using System.Linq;
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

            Loaded += (_, _) => QueryList();

            (s_bl.Order as IObservable)?.AddObserver(QueryList);
        }

        private void QueryList()
        {
            try
            {
                BO.OrderType? typeFilter = null;
                var typeItem = cmbType?.SelectedItem;
                if (typeItem is System.Windows.Controls.ComboBoxItem cb && cb.Content?.ToString() != "All")
                {
                    if (Enum.TryParse(cb.Content?.ToString(), out BO.OrderType parsed)) typeFilter = parsed;
                }

                BO.DeliveryStatus? statusFilter = null;
                var statusItem = cmbEndStatus?.SelectedItem as System.Windows.Controls.ComboBoxItem;
                if (statusItem != null && statusItem.Content?.ToString() != "All")
                {
                    if (Enum.TryParse(statusItem.Content?.ToString(), out BO.DeliveryStatus st)) statusFilter = st;
                }

                string? sortTag = null;
                if (cmbSort?.SelectedItem is System.Windows.Controls.ComboBoxItem sortItem)
                {
                    sortTag = sortItem.Tag as string;
                }

                var list = s_bl.Order.GetClosedOrdersForCourier(_userId, _courierId, typeFilter, null) ?? Enumerable.Empty<ClosedDeliveryInList>();

                if (statusFilter != null)
                {
                    list = list.Where(d => d.DeliveryEndStatus == statusFilter);
                }

                list = sortTag switch
                {
                    "processing" => list.OrderBy(d => d.ProcessingTime),
                    "distance" => list.OrderBy(d => d.ActualDistance ?? double.MaxValue),
                    "status" => list.OrderBy(d => d.DeliveryEndStatus),
                    _ => list.OrderBy(d => d.DeliveryEndStatus)
                };

                ClosedDeliveries = list;
            }
            catch (Exception ex)
            {
                ModernMessageBox.Show($"Error loading closed deliveries: {ex.Message}", "Error", ModernMessageBox.MessageBoxType.Error, ModernMessageBox.MessageBoxButtons.OK, this);
            }
        }

        private void Filter_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }

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
