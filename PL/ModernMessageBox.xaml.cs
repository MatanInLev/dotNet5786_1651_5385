using System.Windows;

namespace PL
{
    /// <summary>
    /// Modern styled message box window
    /// </summary>
    public partial class ModernMessageBox : Window
    {
        public enum MessageBoxType
        {
            Information,
            Success,
            Warning,
            Error,
            Question
        }

        public enum MessageBoxButtons
        {
            OK,
            YesNo
        }

        public bool? DialogResultValue { get; private set; }

        private ModernMessageBox(string message, string title, MessageBoxType type, MessageBoxButtons buttons)
        {
            InitializeComponent();

            TitleText.Text = title;
            MessageText.Text = message;

            // Set icon and color based on type
            switch (type)
            {
                case MessageBoxType.Information:
                    IconText.Text = "??";
                    TitleText.Foreground = System.Windows.Media.Brushes.DodgerBlue;
                    break;
                case MessageBoxType.Success:
                    IconText.Text = "?";
                    TitleText.Foreground = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#27AE60"));
                    break;
                case MessageBoxType.Warning:
                    IconText.Text = "??";
                    TitleText.Foreground = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F39C12"));
                    break;
                case MessageBoxType.Error:
                    IconText.Text = "?";
                    TitleText.Foreground = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E74C3C"));
                    break;
                case MessageBoxType.Question:
                    IconText.Text = "?";
                    TitleText.Foreground = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#9B59B6"));
                    break;
            }

            // Set button visibility based on buttons parameter
            switch (buttons)
            {
                case MessageBoxButtons.OK:
                    OkButton.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButtons.YesNo:
                    YesButton.Visibility = Visibility.Visible;
                    NoButton.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResultValue = true;
            Close();
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResultValue = true;
            Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResultValue = false;
            Close();
        }

        /// <summary>
        /// Show a modern message box
        /// </summary>
        public static bool? Show(string message, string title = "Message", 
            MessageBoxType type = MessageBoxType.Information, 
            MessageBoxButtons buttons = MessageBoxButtons.OK,
            Window? owner = null)
        {
            var msgBox = new ModernMessageBox(message, title, type, buttons);
            
            if (owner != null)
            {
                msgBox.Owner = owner;
            }
            else
            {
                msgBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            msgBox.ShowDialog();
            return msgBox.DialogResultValue;
        }
    }
}
