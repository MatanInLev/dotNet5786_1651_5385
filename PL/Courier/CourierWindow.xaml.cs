using BlApi;
using BO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace PL.Courier
{
    public partial class CourierWindow : Window
    {
        static readonly IBl s_bl = Factory.Get();

        private BO.Courier? _originalCourier; // the source object from BL (for existing courier)

        #region Dependency Properties

        // Property for the text color (White for Delete, Black for Cancel)
        public System.Windows.Media.Brush DeleteButtonForeground
        {
            get { return (System.Windows.Media.Brush)GetValue(DeleteButtonForegroundProperty); }
            set { SetValue(DeleteButtonForegroundProperty, value); }
        }

        public static readonly DependencyProperty DeleteButtonForegroundProperty =
            DependencyProperty.Register(nameof(DeleteButtonForeground), typeof(System.Windows.Media.Brush), typeof(CourierWindow), new PropertyMetadata(System.Windows.Media.Brushes.White));

        // Property for the Delete/Cancel button text
        public string DeleteButtonText
        {
            get { return (string)GetValue(DeleteButtonTextProperty); }
            set { SetValue(DeleteButtonTextProperty, value); }
        } 

        public static readonly DependencyProperty DeleteButtonTextProperty =
            DependencyProperty.Register(nameof(DeleteButtonText), typeof(string), typeof(CourierWindow), new PropertyMetadata("Delete Courier"));

        // Property for the button background color (so Cancel isn't Red)
        public System.Windows.Media.Brush DeleteButtonBackground
        {
            get { return (System.Windows.Media.Brush)GetValue(DeleteButtonBackgroundProperty); }
            set { SetValue(DeleteButtonBackgroundProperty, value); }
        }

        public static readonly DependencyProperty DeleteButtonBackgroundProperty =
            DependencyProperty.Register(nameof(DeleteButtonBackground), typeof(System.Windows.Media.Brush), typeof(CourierWindow), new PropertyMetadata(System.Windows.Media.Brushes.Red));
        public BO.Courier? CurrentCourier
        {
            get { return (BO.Courier?)GetValue(CurrentCourierProperty); }
            set { SetValue(CurrentCourierProperty, value); }
        }

        public static readonly DependencyProperty CurrentCourierProperty =
            DependencyProperty.Register(nameof(CurrentCourier), typeof(BO.Courier), typeof(CourierWindow), new PropertyMetadata(null));

        public string ButtonText
        {
            get { return (string)GetValue(ButtonTextProperty); }
            set { SetValue(ButtonTextProperty, value); }
        }

        public static readonly DependencyProperty ButtonTextProperty =
            DependencyProperty.Register(nameof(ButtonText), typeof(string), typeof(CourierWindow), new PropertyMetadata("Action"));

        // Property to populate the ComboBox with Enum values
        public IEnumerable<BO.Vehicle> VehicleOptions { get; } = Enum.GetValues(typeof(BO.Vehicle)).Cast<BO.Vehicle>();

        #endregion

        public CourierWindow(int courierId = 0)
        {
            int adminId = s_bl.Admin.GetConfig().AdminId;

            (ButtonText, DeleteButtonText, DeleteButtonBackground, DeleteButtonForeground) = (courierId == 0)
            ? ("Add", "Cancel", System.Windows.Media.Brushes.LightGray, System.Windows.Media.Brushes.Black)
            : ("Update", "Delete Courier", System.Windows.Media.Brushes.Red, System.Windows.Media.Brushes.White);

            InitializeComponent();
            Closed += Window_Closed;
            Loaded += Window_Loaded;

            if (courierId != 0)
            {
                // Load original from BL and edit a copy so closing/cancel doesn't commit changes
                _originalCourier = s_bl.Courier.Get(adminId, courierId);
                CurrentCourier = CloneCourier(_originalCourier!);
            }
            else
            {
                // New courier: create editing instance with sensible defaults
                _originalCourier = null;
                CurrentCourier = new BO.Courier()
                {
                    Id = 0,
                    IsActive = true,
                    Vehicle = BO.Vehicle.None,
                    StartWorkDate = s_bl.Admin.GetClock()
                };
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentCourier == null) return;
                if (CurrentCourier.Id == 0)
                {
                    Close();
                    return;
                }
                int adminId = s_bl.Admin.GetConfig().AdminId;
                s_bl.Courier.Delete(adminId, CurrentCourier.Id);
                MessageBox.Show("Courier deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
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
                if (CurrentCourier == null) return;

                int adminId = s_bl.Admin.GetConfig().AdminId;

                if (ButtonText == "Add")
                {
                    s_bl.Courier.Add(adminId, CurrentCourier);

                    MessageBox.Show("Courier added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    s_bl.Courier.Update(adminId, CurrentCourier);
                    MessageBox.Show("Courier updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                Close();
            }
            catch (BlInvalidValueException ex)
            {
                MessageBox.Show($"Invalid Data: {ex.Message}", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (BlAlreadyExistsException ex)
            {
                MessageBox.Show($"ID already exists: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CourierObserver()
        {
            Dispatcher.Invoke(() =>
            {
                if (CurrentCourier?.Id == 0) return;

                try
                {
                    int adminId = s_bl.Admin.GetConfig().AdminId; 
                    CurrentCourier = s_bl.Courier.Get(adminId, CurrentCourier!.Id);
                }
                catch (Exception)
                {
                    Close();
                }
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (CurrentCourier != null && CurrentCourier.Id != 0)
            {
                s_bl.Courier.AddObserver(CurrentCourier.Id, CourierObserver);
            }
        }
        private void Window_Closed(object? sender, EventArgs e)
        {
            if (CurrentCourier != null && CurrentCourier.Id != 0)
            {
                s_bl.Courier.RemoveObserver(CurrentCourier.Id, CourierObserver);
            }
        }

        private BO.Courier CloneCourier(BO.Courier src)
        {
            if (src == null) return new BO.Courier() { Id = 0, IsActive = true, Vehicle = BO.Vehicle.None, StartWorkDate = s_bl.Admin.GetClock() };

            return new BO.Courier()
            {
                Id = src.Id,
                Name = src.Name,
                Phone = src.Phone,
                Email = src.Email,
                IsActive = src.IsActive,
                MaxDistance = src.MaxDistance,
                Vehicle = src.Vehicle,
                StartWorkDate = src.StartWorkDate,
                OrdersProvidedOnTime = src.OrdersProvidedOnTime,
                OrdersProvidedLate = src.OrdersProvidedLate,
                OrderInProgress = src.OrderInProgress
            };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}