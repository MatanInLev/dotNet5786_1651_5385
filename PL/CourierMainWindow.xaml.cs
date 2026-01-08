using BlApi;
using BO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace PL.Courier
{
    public partial class CourierMainWindow : Window
    {
        static readonly IBl s_bl = Factory.Get();

        private int _adminId;
        private int _courierId;
        private bool _observerRegistered = false;

        public BO.Courier? CurrentCourier
        {
            get => (BO.Courier?)GetValue(CurrentCourierProperty);
            set => SetValue(CurrentCourierProperty, value);
        }

        public static readonly DependencyProperty CurrentCourierProperty =
            DependencyProperty.Register(nameof(CurrentCourier), typeof(BO.Courier), typeof(CourierMainWindow), new PropertyMetadata(null, OnCourierChanged));

        public BO.OrderInProgress? CurrentOrderInProgress
        {
            get => (BO.OrderInProgress?)GetValue(CurrentOrderInProgressProperty);
            set => SetValue(CurrentOrderInProgressProperty, value);
        }

        public static readonly DependencyProperty CurrentOrderInProgressProperty =
            DependencyProperty.Register(nameof(CurrentOrderInProgress), typeof(BO.OrderInProgress), typeof(CourierMainWindow), new PropertyMetadata(null, OnOrderChanged));

        public bool CanPickOrder
        {
            get => (bool)GetValue(CanPickOrderProperty);
            set => SetValue(CanPickOrderProperty, value);
        }

        public static readonly DependencyProperty CanPickOrderProperty =
            DependencyProperty.Register(nameof(CanPickOrder), typeof(bool), typeof(CourierMainWindow), new PropertyMetadata(false));

        public bool CanFinishDelivery
        {
            get => (bool)GetValue(CanFinishDeliveryProperty);
            set => SetValue(CanFinishDeliveryProperty, value);
        }

        public static readonly DependencyProperty CanFinishDeliveryProperty =
            DependencyProperty.Register(nameof(CanFinishDelivery), typeof(bool), typeof(CourierMainWindow), new PropertyMetadata(false));

        public IEnumerable<Vehicle> VehiclesList { get; } = Enum.GetValues(typeof(Vehicle)).Cast<Vehicle>();

        public double? EditableMaxDistance { get; set; }
        public Vehicle EditableVehicle { get; set; }

        public CourierMainWindow()
        {
            InitializeComponent();
            Loaded += CourierMainWindow_Loaded;
            Closed += CourierMainWindow_Closed;
        }

        public CourierMainWindow(int adminId, int courierId) : this()
        {
            _adminId = adminId;
            _courierId = courierId;
            Refresh();
        }

        private static void OnCourierChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CourierMainWindow win)
            {
                win.UpdateStates();
            }
        }

        private static void OnOrderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CourierMainWindow win)
            {
                win.UpdateStates();
            }
        }

        private void UpdateStates()
        {
            bool hasOrder = CurrentOrderInProgress != null;
            bool isActive = CurrentCourier?.IsActive ?? false;
            CanPickOrder = isActive && !hasOrder;
            CanFinishDelivery = hasOrder;
        }

        private void Refresh()
        {
            try
            {
                CurrentCourier = s_bl.Courier.Get(_adminId, _courierId);
                CurrentOrderInProgress = CurrentCourier?.OrderInProgress;
                EditableMaxDistance = CurrentCourier?.MaxDistance;
                EditableVehicle = CurrentCourier?.Vehicle ?? VehiclesList.FirstOrDefault();
                DataContext = null;
                DataContext = this;
            }
            catch
            {
                CurrentCourier = null;
                CurrentOrderInProgress = null;
            }
        }

        private void CourierMainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (CurrentCourier != null)
            {
                (s_bl.Courier as IObservable)?.AddObserver(_courierId, OnCourierUpdated);
                _observerRegistered = true;
            }
        }

        private void CourierMainWindow_Closed(object? sender, EventArgs e)
        {
            if (_observerRegistered)
            {
                try
                {
                    (s_bl.Courier as IObservable)?.RemoveObserver(_courierId, OnCourierUpdated);
                }
                catch { }
            }
        }

        private void OnCourierUpdated()
        {
            Dispatcher.Invoke(Refresh);
        }

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new Courier.ClosedDeliveryListWindow(_adminId, _courierId) { Owner = this };
                win.Show();
            }
            catch (Exception ex)
            {
                ModernMessageBox.Show($"Error opening history: {ex.Message}", "Error", ModernMessageBox.MessageBoxType.Error, ModernMessageBox.MessageBoxButtons.OK, this);
            }
        }

        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new PL.Order.OrderListWindow().Show();
            }
            catch (Exception ex)
            {
                ModernMessageBox.Show($"Error opening orders: {ex.Message}", "Error", ModernMessageBox.MessageBoxType.Error, ModernMessageBox.MessageBoxButtons.OK, this);
            }
        }

        private void BtnSelectOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanPickOrder)
                {
                    ModernMessageBox.Show("You cannot pick an order now (inactive or already delivering).", "Info", ModernMessageBox.MessageBoxType.Information, ModernMessageBox.MessageBoxButtons.OK, this);
                    return;
                }

                var win = new Courier.OpenOrdersForCourierWindow(_adminId, _courierId) { Owner = this };
                win.ShowDialog();
                Refresh();
            }
            catch (Exception ex)
            {
                ModernMessageBox.Show($"Error opening orders: {ex.Message}", "Error", ModernMessageBox.MessageBoxType.Error, ModernMessageBox.MessageBoxButtons.OK, this);
            }
        }

        private void BtnFinish_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentOrderInProgress == null)
                {
                    ModernMessageBox.Show("No delivery in progress.", "Info", ModernMessageBox.MessageBoxType.Information, ModernMessageBox.MessageBoxButtons.OK, this);
                    return;
                }

                // Ask status
                var dialog = new Courier.FinishDeliveryDialog();
                dialog.Owner = this;
                if (dialog.ShowDialog() != true)
                    return;

                var status = dialog.SelectedStatus;
                s_bl.Order.CompleteOrderDelivery(_adminId, CurrentOrderInProgress.DeliveryId, status);
                ModernMessageBox.Show("Delivery completed.", "Success", ModernMessageBox.MessageBoxType.Success, ModernMessageBox.MessageBoxButtons.OK, this);
                Refresh();
            }
            catch (Exception ex)
            {
                ModernMessageBox.Show($"Error while finishing: {ex.Message}", "Error", ModernMessageBox.MessageBoxType.Error, ModernMessageBox.MessageBoxButtons.OK, this);
            }
        }

        private void BtnUpdateCourier_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentCourier == null)
                {
                    ModernMessageBox.Show("No courier loaded.", "Error", ModernMessageBox.MessageBoxType.Error, ModernMessageBox.MessageBoxButtons.OK, this);
                    return;
                }

                // Validate vehicle change when delivery in progress
                if (CurrentOrderInProgress != null && EditableVehicle != CurrentCourier.Vehicle)
                {
                    ModernMessageBox.Show("Cannot change vehicle while a delivery is in progress.", "Validation", ModernMessageBox.MessageBoxType.Warning, ModernMessageBox.MessageBoxButtons.OK, this);
                    return;
                }

                // Validate max distance vs company max
                var cfg = s_bl.Admin.GetConfig();
                if (cfg.MaxRange > 0 && EditableMaxDistance.HasValue && EditableMaxDistance.Value > cfg.MaxRange)
                {
                    ModernMessageBox.Show($"Max distance cannot exceed company limit ({cfg.MaxRange:F2} km).", "Validation", ModernMessageBox.MessageBoxType.Warning, ModernMessageBox.MessageBoxButtons.OK, this);
                    return;
                }

                var updated = new BO.Courier
                {
                    Id = CurrentCourier.Id,
                    Name = CurrentCourier.Name,
                    Phone = CurrentCourier.Phone,
                    Email = CurrentCourier.Email,
                    IsActive = CurrentCourier.IsActive,
                    MaxDistance = EditableMaxDistance,
                    Vehicle = EditableVehicle,
                    StartWorkDate = CurrentCourier.StartWorkDate,
                    OrdersProvidedOnTime = CurrentCourier.OrdersProvidedOnTime,
                    OrdersProvidedLate = CurrentCourier.OrdersProvidedLate,
                    OrderInProgress = CurrentCourier.OrderInProgress
                };

                s_bl.Courier.Update(_adminId, updated);
                ModernMessageBox.Show("Profile updated.", "Success", ModernMessageBox.MessageBoxType.Success, ModernMessageBox.MessageBoxButtons.OK, this);
                Refresh();
            }
            catch (Exception ex)
            {
                ModernMessageBox.Show($"Update failed: {ex.Message}", "Error", ModernMessageBox.MessageBoxType.Error, ModernMessageBox.MessageBoxButtons.OK, this);
            }
        }
    }
}
