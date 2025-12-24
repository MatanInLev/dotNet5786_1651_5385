using BlApi;
using BO;
using System;
using System.Windows;
using System.Windows.Media;

namespace PL.Order
{
    /// <summary>
    /// Interaction logic for OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : Window
    {
        static readonly IBl s_bl = Factory.Get();

        private BO.Order? _originalOrder; // the source object from BL (for existing order)

        #region Dependency Properties

        // Property for the text color (White for Delete, Black for Cancel)
        public Brush DeleteButtonForeground
        {
            get { return (Brush)GetValue(DeleteButtonForegroundProperty); }
            set { SetValue(DeleteButtonForegroundProperty, value); }
        }

        public static readonly DependencyProperty DeleteButtonForegroundProperty =
            DependencyProperty.Register(nameof(DeleteButtonForeground), typeof(Brush), typeof(OrderWindow), new PropertyMetadata(Brushes.White));

        // Property for the Delete/Cancel button text
        public string DeleteButtonText
        {
            get { return (string)GetValue(DeleteButtonTextProperty); }
            set { SetValue(DeleteButtonTextProperty, value); }
        }

        public static readonly DependencyProperty DeleteButtonTextProperty =
            DependencyProperty.Register(nameof(DeleteButtonText), typeof(string), typeof(OrderWindow), new PropertyMetadata("Delete Order"));

        // Property for the button background color (so Cancel isn't Red)
        public Brush DeleteButtonBackground
        {
            get { return (Brush)GetValue(DeleteButtonBackgroundProperty); }
            set { SetValue(DeleteButtonBackgroundProperty, value); }
        }

        public static readonly DependencyProperty DeleteButtonBackgroundProperty =
            DependencyProperty.Register(nameof(DeleteButtonBackground), typeof(Brush), typeof(OrderWindow), new PropertyMetadata(Brushes.Red));

        public BO.Order? CurrentOrder
        {
            get { return (BO.Order?)GetValue(CurrentOrderProperty); }
            set { SetValue(CurrentOrderProperty, value); }
        }

        public static readonly DependencyProperty CurrentOrderProperty =
            DependencyProperty.Register(nameof(CurrentOrder), typeof(BO.Order), typeof(OrderWindow), new PropertyMetadata(null));

        public string ButtonText
        {
            get { return (string)GetValue(ButtonTextProperty); }
            set { SetValue(ButtonTextProperty, value); }
        }

        public static readonly DependencyProperty ButtonTextProperty =
            DependencyProperty.Register(nameof(ButtonText), typeof(string), typeof(OrderWindow), new PropertyMetadata("Action"));

        #endregion

        public OrderWindow(int orderId = 0)
        {
            int adminId = s_bl.Admin.GetConfig().AdminId;

            (ButtonText, DeleteButtonText, DeleteButtonBackground, DeleteButtonForeground) = (orderId == 0)
            ? ("Add", "Cancel", Brushes.LightGray, Brushes.Black)
            : ("Update", "Delete Order", Brushes.Red, Brushes.White);

            InitializeComponent();
            Closed += Window_Closed;
            Loaded += Window_Loaded;

            if (orderId != 0)
            {
                // Load original from BL and edit a copy so closing/cancel doesn't commit changes
                _originalOrder = s_bl.Order.Get(adminId, orderId);
                CurrentOrder = CloneOrder(_originalOrder!);
            }
            else
            {
                // New order: create editing instance with sensible defaults
                _originalOrder = null;
                CurrentOrder = new BO.Order()
                {
                    Id = 0,
                    Type = OrderType.Burger,
                    CustomerAddress = string.Empty,
                    CustomerName = string.Empty,
                    CustomerPhone = string.Empty,
                    OrderDate = s_bl.Admin.GetClock()
                };
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentOrder == null) return;
                if (CurrentOrder.Id == 0)
                {
                    Close();
                    return;
                }

                var result = MessageBox.Show(
                    $"Are you sure you want to delete order {CurrentOrder.Id}?",
                    "Confirm Deletion",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    int adminId = s_bl.Admin.GetConfig().AdminId;
                    s_bl.Order.Delete(adminId, CurrentOrder.Id);
                    MessageBox.Show("Order deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }
            }
            catch (BlInvalidValueException ex)
            {
                MessageBox.Show($"Invalid Data: {ex.Message}", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the Add/Update button click.
        /// Commits only when the user clicks the button (save-on-demand).
        /// </summary>
        private void btnAction_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentOrder == null) return;

                int adminId = s_bl.Admin.GetConfig().AdminId;

                if (ButtonText == "Add")
                {
                    s_bl.Order.Add(adminId, CurrentOrder);
                    MessageBox.Show("Order added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    s_bl.Order.Update(adminId, CurrentOrder);
                    MessageBox.Show("Order updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                Close();
            }
            catch (BlInvalidValueException ex)
            {
                MessageBox.Show($"Invalid Data: {ex.Message}", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (BlAlreadyExistsException ex)
            {
                MessageBox.Show($"Order already exists: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OrderObserver()
        {
            Dispatcher.Invoke(() =>
            {
                if (CurrentOrder?.Id == 0) return;

                try
                {
                    int adminId = s_bl.Admin.GetConfig().AdminId;
                    CurrentOrder = s_bl.Order.Get(adminId, CurrentOrder!.Id);
                }
                catch (Exception)
                {
                    Close();
                }
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (CurrentOrder != null && CurrentOrder.Id != 0)
            {
                (s_bl.Order as IObservable)?.AddObserver(OrderObserver);
            }
        }

        private void Window_Closed(object? sender, EventArgs e)
        {
            if (CurrentOrder != null && CurrentOrder.Id != 0)
            {
                (s_bl.Order as IObservable)?.RemoveObserver(OrderObserver);
            }
        }

        private BO.Order CloneOrder(BO.Order src)
        {
            if (src == null)
            {
                return new BO.Order()
                {
                    Id = 0,
                    Type = OrderType.Burger,
                    CustomerAddress = string.Empty,
                    CustomerName = string.Empty,
                    CustomerPhone = string.Empty,
                    OrderDate = s_bl.Admin.GetClock()
                };
            }

            return new BO.Order()
            {
                Id = src.Id,
                Type = src.Type,
                Description = src.Description,
                CustomerAddress = src.CustomerAddress,
                Latitude = src.Latitude,
                Longitude = src.Longitude,
                Distance = src.Distance,
                CustomerName = src.CustomerName,
                CustomerPhone = src.CustomerPhone,
                OrderDate = src.OrderDate,
                ExpectedDelivery = src.ExpectedDelivery,
                MaxDelivery = src.MaxDelivery,
                Status = src.Status,
                ScheduleStatus = src.ScheduleStatus,
                TimeLeft = src.TimeLeft,
                DeliveryList = src.DeliveryList
            };
        }
    }
}
