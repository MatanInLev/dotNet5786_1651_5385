using System;
using System.Windows;
using BlApi;
using BO;

namespace PL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Access to the Business Logic layer via the Factory.
        /// </summary>
        static readonly IBl s_bl = Factory.Get();

        #region Dependency Properties

        // --- CurrentTime ---
        /// <summary>
        /// The current system time exposed to the UI.
        /// </summary>
        public DateTime CurrentTime
        {
            get { return (DateTime)GetValue(CurrentTimeProperty); }
            set { SetValue(CurrentTimeProperty, value); }
        }

        /// <summary>
        /// DependencyProperty backing store for <see cref="CurrentTime"/>.
        /// </summary>
        public static readonly DependencyProperty CurrentTimeProperty =
            DependencyProperty.Register(nameof(CurrentTime), typeof(DateTime), typeof(MainWindow), new PropertyMetadata(DateTime.Now));

        // --- Configuration ---
        /// <summary>
        /// The current system configuration exposed to the UI.
        /// </summary>
        public BO.Config Configuration
        {
            get { return (BO.Config)GetValue(ConfigurationProperty); }
            set { SetValue(ConfigurationProperty, value); }
        }

        // Fixed: Default value changed from '0' to 'null' to prevent type mismatch crash
        /// <summary>
        /// DependencyProperty backing store for <see cref="Configuration"/>.
        /// </summary>
        public static readonly DependencyProperty ConfigurationProperty =
            DependencyProperty.Register(nameof(Configuration), typeof(BO.Config), typeof(MainWindow), new PropertyMetadata(null));

        #endregion

        #region Constructor & Window Events

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Register for Window Life-cycle events
            Loaded += Window_Loaded;
            Closed += Window_Closed;
        }

        /// <summary>
        /// Handles the Loaded event; initializes time and configuration and registers observers.
        /// </summary>
        private void Window_Loaded(object? sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Load initial data
                CurrentTime = s_bl.Admin.GetClock();
                Configuration = s_bl.Admin.GetConfig();

                s_bl.Admin.AddClockObserver(clockObserver);
                s_bl.Admin.AddConfigObserver(configObserver);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the Closed event; removes observers.
        /// </summary>
        private void Window_Closed(object? sender, EventArgs e)
        {
            s_bl.Admin.RemoveClockObserver(clockObserver);
            s_bl.Admin.RemoveConfigObserver(configObserver);
        }

        #endregion

        #region Observer Methods

        /// <summary>
        /// Observer called by BL when the clock is updated; updates the UI thread-safe.
        /// </summary>
        private void clockObserver()
        {
            // Dispatcher.Invoke ensures we update the UI thread correctly
            Dispatcher.Invoke(() => CurrentTime = s_bl.Admin.GetClock());
        }

        /// <summary>
        /// Observer called by BL when configuration is updated; updates the UI thread-safe.
        /// </summary>
        private void configObserver()
        {
            Dispatcher.Invoke(() => Configuration = s_bl.Admin.GetConfig());
        }

        #endregion

        #region Clock Buttons (Forward Clock)

        /// <summary>Advance clock by one minute.</summary>
        private void btnAddOneMinute_click(object sender, RoutedEventArgs e) => s_bl.Admin.ForwardClock(TimeUnit.Minutes);
        /// <summary>Advance clock by one hour.</summary>
        private void btnAddOneHour_click(object sender, RoutedEventArgs e) => s_bl.Admin.ForwardClock(TimeUnit.Hours);
        /// <summary>Advance clock by one day.</summary>
        private void btnAddOneDay_click(object sender, RoutedEventArgs e) => s_bl.Admin.ForwardClock(TimeUnit.Days);
        /// <summary>Advance clock by one month.</summary>
        private void btnAddOneMonth_click(object sender, RoutedEventArgs e) => s_bl.Admin.ForwardClock(TimeUnit.Months);
        /// <summary>Advance clock by one year.</summary>
        private void btnAddOneYear_click(object sender, RoutedEventArgs e) => s_bl.Admin.ForwardClock(TimeUnit.Years);

        #endregion

        #region Management Buttons

        /// <summary>
        /// Applies configuration changes to the business layer.
        /// </summary>
        private void btnUpdateConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Configuration != null)
                {
                    s_bl.Admin.SetConfig(Configuration);
                    MessageBox.Show("Configuration updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Update Failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Initializes the database via business layer (confirmation required).
        /// </summary>
        private void btnInitDB_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to initialize the DB? This will delete existing data.", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    s_bl.Admin.InitializeDB();
                    MessageBox.Show("Database Initialized.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Initialization Failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Resets the database via business layer (irreversible, confirmation required).
        /// </summary>
        private void btnResetDB_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to WIPE the DB? This cannot be undone.", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    s_bl.Admin.ResetDB();
                    MessageBox.Show("Database Reset.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Reset Failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Opens the courier list window.
        /// </summary>
        private void btnOpenList_Click(object sender, RoutedEventArgs e)
        {
            // Replace 'Courier.CourierListWindow' with the correct namespace/path to your list window
            new Courier.CourierListWindow().Show();
        }

        #endregion
    }
}