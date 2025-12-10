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


        #region Dependency Properties

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
            ButtonText = (courierId == 0) ? "Add" : "Update";
            int adminId = s_bl.Admin.GetConfig().AdminId;
            InitializeComponent();
            Closed += Window_Closed;
            Loaded += Window_Loaded;
            CurrentCourier = (courierId != 0) ? s_bl.Courier.Get(adminId, courierId)! : new BO.Courier()
            {
                Id = 0,
                IsActive = true,
                Vehicle = BO.Vehicle.None,
                StartWorkDate = s_bl.Admin.GetClock()
            };

        }

        /// <summary>
        /// Handles the Add/Update button click.
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
    }
}