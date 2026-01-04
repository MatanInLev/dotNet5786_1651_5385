using BlApi;
using BO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PL.Order
{
    /// <summary>
    /// Interaction logic for OrderListWindow.xaml
    /// </summary>
    public partial class OrderListWindow : Window
    {
        static readonly IBl s_bl = Factory.Get();

        public BO.OrderInList? SelectedOrder { get; set; }

        /// <summary>
        /// The list of orders bound to the DataGrid
        /// </summary>
        public IEnumerable<BO.OrderInList> OrderList
        {
            get { return (IEnumerable<BO.OrderInList>)GetValue(OrderListProperty); }
            set { SetValue(OrderListProperty, value); }
        }

        public static readonly DependencyProperty OrderListProperty =
            DependencyProperty.Register(nameof(OrderList), typeof(IEnumerable<BO.OrderInList>), typeof(OrderListWindow), new PropertyMetadata(null));

        /// <summary>
        /// Selected filter for Order Type (nullable for "All")
        /// </summary>
        public BO.OrderType? SelectedOrderType
        {
            get { return (BO.OrderType?)GetValue(SelectedOrderTypeProperty); }
            set { SetValue(SelectedOrderTypeProperty, value); }
        }

        public static readonly DependencyProperty SelectedOrderTypeProperty =
            DependencyProperty.Register(nameof(SelectedOrderType), typeof(BO.OrderType?), typeof(OrderListWindow), new PropertyMetadata(null));

        /// <summary>
        /// Selected filter for Order Status (nullable for "All")
        /// </summary>
        public BO.OrderStatus? SelectedOrderStatus
        {
            get { return (BO.OrderStatus?)GetValue(SelectedOrderStatusProperty); }
            set { SetValue(SelectedOrderStatusProperty, value); }
        }

        public static readonly DependencyProperty SelectedOrderStatusProperty =
            DependencyProperty.Register(nameof(SelectedOrderStatus), typeof(BO.OrderStatus?), typeof(OrderListWindow), new PropertyMetadata(null));

        public OrderListWindow()
        {
            InitializeComponent();
            Closed += Window_Closed;
            
            // Initial load
            QueryOrderList();

            // Subscribe to updates (if BL supports observers)
            (s_bl.Order as IObservable)?.AddObserver(QueryOrderList);

            // Initialize status combobox to "All"
            if (cmbOrderStatus != null)
                cmbOrderStatus.SelectedIndex = 0;
        }

        /// <summary>
        /// Queries the order list from BL with current filters and updates the display
        /// </summary>
        private void QueryOrderList()
        {
            try
            {
                int adminId = s_bl.Admin.GetConfig().AdminId;

                // If SelectedOrderStatus is null -> show all orders
                var statusFilter = SelectedOrderStatus;

                // GetList signature: GetList(int userId, BO.OrderStatus? filter, object? filterValue, BO.OrderType? sort)
                var orders = s_bl.Order.GetList(adminId, statusFilter, null, SelectedOrderType);

                OrderList = orders;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading orders: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Called when filter selection changes
        /// </summary>
        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If status combobox: first item is 'All' (ComboBoxItem) -> set SelectedOrderStatus = null
            if (sender == cmbOrderStatus)
            {
                if (cmbOrderStatus.SelectedItem is ComboBoxItem)
                {
                    SelectedOrderStatus = null;
                }
                else
                {
                    // Selected is one of the enum values
                    SelectedOrderStatus = (BO.OrderStatus?)cmbOrderStatus.SelectedItem;
                }
            }

            QueryOrderList();
        }

        /// <summary>
        /// Clear all filters and reload
        /// </summary>
        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            SelectedOrderType = null;
            SelectedOrderStatus = null;

            if (cmbOrderStatus != null) cmbOrderStatus.SelectedIndex = 0;

            QueryOrderList();
        }

        /// <summary>
        /// Double-click handler to open order details window
        /// </summary>
        private void OrderList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;

            var frameworkElement = e.OriginalSource as FrameworkElement;
            var selectedItem = frameworkElement?.DataContext as BO.OrderInList;

            if (selectedItem == null) return;

            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, new Action(() =>
            {
                try
                {
                    new OrderWindow(selectedItem.Id).ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening order window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }));
        }

        /// <summary>
        /// Add new order button handler
        /// </summary>
        private void AddOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new OrderWindow().ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Cancel order button handler
        /// </summary>
        private void CancelOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is not Button button) return;
                if (button.Tag is not int orderId) return;

                var result = MessageBox.Show(
                    $"Are you sure you want to cancel order {orderId}?",
                    "Confirm Cancellation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    int adminId = s_bl.Admin.GetConfig().AdminId;
                    s_bl.Order.Cancel(adminId, orderId);

                    MessageBox.Show("Order cancelled successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    QueryOrderList(); // Refresh the list
                }
            }
            catch (BlInvalidValueException ex)
            {
                MessageBox.Show($"Cannot cancel order: {ex.Message}", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cancelling order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Closed(object? sender, EventArgs e)
        {
            (s_bl.Order as IObservable)?.RemoveObserver(QueryOrderList);
        }
    }
}
