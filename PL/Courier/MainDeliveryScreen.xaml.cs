using System;
using System.Windows;
using System.Windows.Input;

namespace PL.Courier;
public partial class MainDeliveryScreen : Window
{
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    public BO.Courier? CurrentCourier
    {
        get => (BO.Courier?)GetValue(CurrentCourierProperty);
        set => SetValue(CurrentCourierProperty, value);
    }

    public static readonly DependencyProperty CurrentCourierProperty =
        DependencyProperty.Register(nameof(CurrentCourier), typeof(BO.Courier), typeof(MainDeliveryScreen), new PropertyMetadata(null));

    public MainDeliveryScreen()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void btnUpdate_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (CurrentCourier != null && CurrentCourier.Id != 0)
            {
                // Open existing courier window (matches CourierWindow usage)
                new CourierWindow(CurrentCourier.Id).Show();
                return;
            }

            // If no courier selected, open add courier window
            new CourierWindow().Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening courier window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void btnFinish_Click(object sender, RoutedEventArgs e)
    {
        // Placeholder: implement BL call for finishing handling
        MessageBox.Show("Finish handling action triggered.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void btnDetails_Click(object sender, RoutedEventArgs e)
    {
        // Placeholder: show details (could open a details window)
        MessageBox.Show("Show handling details action triggered.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void btnMarkCompleted_Click(object sender, RoutedEventArgs e)
    {
        // Placeholder: mark delivery completed via BL
        MessageBox.Show("Mark as delivery completed action triggered.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void btnHistory_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Open delivery history.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void btnSelect_Click(object sender, RoutedEventArgs e)
    {
        // Example: open CourierListWindow to pick a courier, similar to existing list window
        try
        {
            var list = new CourierListWindow();
            list.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening list window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}