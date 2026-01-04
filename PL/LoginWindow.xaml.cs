using BlApi;
using BO;
using PL.Courier;
using System;
using System.Windows;
using System.Windows.Input;

namespace PL
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private static readonly IBl s_bl = Factory.Get();
        private const string PlaceholderText = "Your ID";

        public LoginWindow()
        {
            InitializeComponent();
            
            // Set placeholder text
            IdTextBox.Text = PlaceholderText;
            IdTextBox.Foreground = System.Windows.Media.Brushes.Gray;
        }

        private void IdTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (IdTextBox.Text == PlaceholderText)
            {
                IdTextBox.Text = "";
                IdTextBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void IdTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(IdTextBox.Text))
            {
                IdTextBox.Text = PlaceholderText;
                IdTextBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void IdTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ConnectButton_Click(sender, e);
            }
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the ID from the text box
                string idText = IdTextBox.Text.Trim();

                // Validate input
                if (string.IsNullOrEmpty(idText) || idText == PlaceholderText)
                {
                    MessageBox.Show("Please enter your ID.", "Invalid Input", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Try to parse the ID
                if (!int.TryParse(idText, out int userId))
                {
                    MessageBox.Show("ID must be a valid number.", "Invalid Input", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Get admin ID from configuration
                int adminId = s_bl.Admin.GetConfig().AdminId;

                // Check if it's the admin
                if (userId == adminId)
                {
                    // Open MainWindow for admin
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                    return;
                }

                // Try to find courier with this ID
                try
                {
                    var courier = s_bl.Courier.Get(adminId, userId);
                    
                    if (courier != null)
                    {
                        // Open CourierMainWindow for courier
                        CourierMainWindow courierWindow = new CourierMainWindow(userId);
                        courierWindow.Show();
                        this.Close();
                        return;
                    }
                }
                catch (BlDoesNotExistException)
                {
                    // Courier not found - show error message
                    MessageBox.Show($"No account found with ID: {userId}\n\nPlease check your ID and try again.", 
                        "Login Failed", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login error: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
