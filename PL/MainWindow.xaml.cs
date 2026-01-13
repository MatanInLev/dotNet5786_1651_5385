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

        /// <summary>
        /// Observer mutex to prevent concurrent observer callbacks - Stage 7
        /// </summary>
        private readonly ObserverMutex _observerMutex = new(); //stage 7

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

        // --- Database Status ---
        /// <summary>
        /// Status message for database operations.
        /// </summary>
        public string DatabaseStatus
        {
            get { return (string)GetValue(DatabaseStatusProperty); }
            set { SetValue(DatabaseStatusProperty, value); }
        }

        /// <summary>
        /// DependencyProperty backing store for <see cref="DatabaseStatus"/>.
        /// </summary>
        public static readonly DependencyProperty DatabaseStatusProperty =
            DependencyProperty.Register(nameof(DatabaseStatus), typeof(string), typeof(MainWindow), new PropertyMetadata(string.Empty));

        // --- Database Buttons Enabled ---
        /// <summary>
        /// Controls whether database buttons are enabled.
        /// </summary>
        public bool DatabaseButtonsEnabled
        {
            get { return (bool)GetValue(DatabaseButtonsEnabledProperty); }
            set { SetValue(DatabaseButtonsEnabledProperty, value); }
        }

        /// <summary>
        /// DependencyProperty backing store for <see cref="DatabaseButtonsEnabled"/>.
        /// </summary>
        public static readonly DependencyProperty DatabaseButtonsEnabledProperty =
            DependencyProperty.Register(nameof(DatabaseButtonsEnabled), typeof(bool), typeof(MainWindow), new PropertyMetadata(true));

        // --- Simulator Status - Stage 7 ---
        /// <summary>
        /// Status message for the simulator.
        /// </summary>
        public string SimulatorStatus
        {
            get { return (string)GetValue(SimulatorStatusProperty); }
            set { SetValue(SimulatorStatusProperty, value); }
        }

        public static readonly DependencyProperty SimulatorStatusProperty =
            DependencyProperty.Register(nameof(SimulatorStatus), typeof(string), typeof(MainWindow), new PropertyMetadata("Simulator Stopped"));

        // --- Simulator Interval - Stage 7 ---
        /// <summary>
        /// Simulator interval in minutes per second.
        /// </summary>
        public int SimulatorInterval
        {
            get { return (int)GetValue(SimulatorIntervalProperty); }
            set { SetValue(SimulatorIntervalProperty, value); }
        }

        public static readonly DependencyProperty SimulatorIntervalProperty =
            DependencyProperty.Register(nameof(SimulatorInterval), typeof(int), typeof(MainWindow), new PropertyMetadata(1));

        // --- Simulator Button Text - Stage 7 ---
        /// <summary>
        /// Text for the simulator toggle button.
        /// </summary>
        public string SimulatorButtonText
        {
            get { return (string)GetValue(SimulatorButtonTextProperty); }
            set { SetValue(SimulatorButtonTextProperty, value); }
        }

        public static readonly DependencyProperty SimulatorButtonTextProperty =
            DependencyProperty.Register(nameof(SimulatorButtonText), typeof(string), typeof(MainWindow), new PropertyMetadata("Start Simulator"));

        // --- Simulator Button Background - Stage 7 ---
        /// <summary>
        /// Background color for the simulator toggle button.
        /// </summary>
        public System.Windows.Media.Brush SimulatorButtonBackground
        {
            get { return (System.Windows.Media.Brush)GetValue(SimulatorButtonBackgroundProperty); }
            set { SetValue(SimulatorButtonBackgroundProperty, value); }
        }

        public static readonly DependencyProperty SimulatorButtonBackgroundProperty =
            DependencyProperty.Register(nameof(SimulatorButtonBackground), typeof(System.Windows.Media.Brush), typeof(MainWindow), 
                new PropertyMetadata(System.Windows.Media.Brushes.Green));

        // --- Simulator Running Flag - Stage 7 ---
        private bool _simulatorRunning = false;

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
                ModernMessageBox.Show($"Error loading window: {ex.Message}", "Error", 
                    ModernMessageBox.MessageBoxType.Error, ModernMessageBox.MessageBoxButtons.OK, this);
            }
        }

        /// <summary>
        /// Handles the Closed event; removes observers.
        /// /// </summary>
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
            // Stage 7: Check if already processing - if so, exit immediately
            if (_observerMutex.CheckAndSetInProgress())
                return;

            try
            {
                // Dispatcher.Invoke ensures we update the UI thread correctly
                Dispatcher.Invoke(() => CurrentTime = s_bl.Admin.GetClock());
            }
            finally
            {
                _observerMutex.UnsetInProgress();
            }
        }

        /// <summary>
        /// Observer called by BL when configuration is updated; updates the UI thread-safe.
        /// </summary>
        private void configObserver()
        {
            // Stage 7: Check if already processing - if so, exit immediately
            if (_observerMutex.CheckAndSetInProgress())
                return;

            try
            {
                Dispatcher.Invoke(() => Configuration = s_bl.Admin.GetConfig());
            }
            finally
            {
                _observerMutex.UnsetInProgress();
            }
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
                    ModernMessageBox.Show("Configuration updated successfully!", "Success", 
                        ModernMessageBox.MessageBoxType.Success, ModernMessageBox.MessageBoxButtons.OK, this);
                }
            }
            catch (Exception ex)
            {
                ModernMessageBox.Show($"Update Failed: {ex.Message}", "Error", 
                    ModernMessageBox.MessageBoxType.Error, ModernMessageBox.MessageBoxButtons.OK, this);
            }
        }

        /// <summary>
        /// Initializes the database via business layer (confirmation required).
        /// </summary>
        private async void btnInitDB_Click(object sender, RoutedEventArgs e)
        {
            var result = ModernMessageBox.Show(
                "Are you sure you want to initialize the database? This will delete existing data and create new sample data.", 
                "Confirm Initialization", 
                ModernMessageBox.MessageBoxType.Question, 
                ModernMessageBox.MessageBoxButtons.YesNo, 
                this);

            if (result == true)
            {
                try
                {
                    // Disable buttons during operation
                    DatabaseButtonsEnabled = false;
                    DatabaseStatus = "Starting database initialization...";

                    await System.Threading.Tasks.Task.Run(() =>
                    {
                        // Update status on UI thread
                        Dispatcher.Invoke(() => DatabaseStatus = "Resetting database...");
                        System.Threading.Thread.Sleep(300); // Small delay for UI update visibility

                        Dispatcher.Invoke(() => DatabaseStatus = "Initializing couriers...");
                        System.Threading.Thread.Sleep(300);

                        Dispatcher.Invoke(() => DatabaseStatus = "Initializing orders...");
                        System.Threading.Thread.Sleep(300);

                        Dispatcher.Invoke(() => DatabaseStatus = "Creating deliveries...");
                        System.Threading.Thread.Sleep(300);

                        Dispatcher.Invoke(() => DatabaseStatus = "Finalizing initialization...");
                        
                        // Actual initialization
                        s_bl.Admin.InitializeDB();
                    });

                    DatabaseStatus = "Database initialized successfully!";
                    await System.Threading.Tasks.Task.Delay(2000); // Show success message for 2 seconds
                    DatabaseStatus = string.Empty;

                    ModernMessageBox.Show("Database initialized successfully!", "Success", 
                        ModernMessageBox.MessageBoxType.Success, ModernMessageBox.MessageBoxButtons.OK, this);
                }
                catch (Exception ex)
                {
                    DatabaseStatus = $"Error: {ex.Message}";
                    ModernMessageBox.Show($"Initialization Failed: {ex.Message}", "Error", 
                        ModernMessageBox.MessageBoxType.Error, ModernMessageBox.MessageBoxButtons.OK, this);
                }
                finally
                {
                    // Re-enable buttons
                    DatabaseButtonsEnabled = true;
                }
            }
        }

        /// <summary>
        /// Resets the database via business layer (irreversible, confirmation required).
        /// </summary>
        private async void btnResetDB_Click(object sender, RoutedEventArgs e)
        {
            var result = ModernMessageBox.Show(
                "⚠️ WARNING: This will completely WIPE the database and reset all configuration to defaults. This action CANNOT be undone!\n\nAre you absolutely sure?", 
                "Confirm Database Reset", 
                ModernMessageBox.MessageBoxType.Warning, 
                ModernMessageBox.MessageBoxButtons.YesNo, 
                this);

            if (result == true)
            {
                try
                {
                    // Disable buttons during operation
                    DatabaseButtonsEnabled = false;
                    DatabaseStatus = "Starting database reset...";

                    await System.Threading.Tasks.Task.Run(() =>
                    {
                        Dispatcher.Invoke(() => DatabaseStatus = "Clearing couriers...");
                        System.Threading.Thread.Sleep(300);

                        Dispatcher.Invoke(() => DatabaseStatus = "Clearing orders...");
                        System.Threading.Thread.Sleep(300);

                        Dispatcher.Invoke(() => DatabaseStatus = "Clearing deliveries...");
                        System.Threading.Thread.Sleep(300);

                        Dispatcher.Invoke(() => DatabaseStatus = "Resetting system configuration...");
                        
                        // Actual reset
                        s_bl.Admin.ResetDB();
                    });

                    DatabaseStatus = "Database reset complete!";
                    await System.Threading.Tasks.Task.Delay(2000); // Show success message for 2 seconds
                    DatabaseStatus = string.Empty;

                    ModernMessageBox.Show("Database has been reset to factory defaults.", "Success", 
                        ModernMessageBox.MessageBoxType.Success, ModernMessageBox.MessageBoxButtons.OK, this);
                }
                catch (Exception ex)
                {
                    DatabaseStatus = $"Error: {ex.Message}";
                    ModernMessageBox.Show($"Reset Failed: {ex.Message}", "Error", 
                        ModernMessageBox.MessageBoxType.Error, ModernMessageBox.MessageBoxButtons.OK, this);
                }
                finally
                {
                    // Re-enable buttons
                    DatabaseButtonsEnabled = true;
                }
            }
        }

        /// <summary>
        /// Opens the courier list window.
        /// </summary>
        private void btnOpenCourierList_Click(object sender, RoutedEventArgs e)
        {
            new Courier.CourierListWindow().Show();
        }

        /// <summary>
        /// Opens the order list window.
        /// </summary>
        private void btnOpenOrderList_Click(object sender, RoutedEventArgs e)
        {
            new Order.OrderListWindow().Show();
        }

        #endregion

        #region Simulator Control - Stage 7

        /// <summary>
        /// Toggles the simulator on/off.
        /// </summary>
        private void btnToggleSimulator_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_simulatorRunning)
                {
                    // Validate interval
                    if (SimulatorInterval <= 0)
                    {
                        ModernMessageBox.Show("Please enter a valid interval (greater than 0).", "Invalid Interval",
                            ModernMessageBox.MessageBoxType.Warning, ModernMessageBox.MessageBoxButtons.OK, this);
                        return;
                    }

                    // Start simulator
                    s_bl.Admin.StartSimulator(SimulatorInterval);
                    
                    _simulatorRunning = true;
                    SimulatorStatus = $"Simulator Running ({SimulatorInterval} min/sec)";
                    SimulatorButtonText = "Stop Simulator";
                    SimulatorButtonBackground = System.Windows.Media.Brushes.Red;
                    DatabaseButtonsEnabled = false; // Disable database operations while simulator runs
                    
                    ModernMessageBox.Show(
                        $"Simulator started!\n\n" +
                        $"• Clock advances {SimulatorInterval} min/sec\n" +
                        $"• Orders auto-assigned\n" +
                        $"• Deliveries auto-completed", 
                        "Simulator Started", 
                        ModernMessageBox.MessageBoxType.Success, 
                        ModernMessageBox.MessageBoxButtons.OK, 
                        this);
                }
                else
                {
                    // Stop simulator
                    s_bl.Admin.StopSimulator();
                    
                    _simulatorRunning = false;
                    SimulatorStatus = "Simulator Stopped";
                    SimulatorButtonText = "Start Simulator";
                    SimulatorButtonBackground = System.Windows.Media.Brushes.Green;
                    DatabaseButtonsEnabled = true; // Re-enable database operations
                    
                    ModernMessageBox.Show("Simulator stopped.", "Simulator Stopped", 
                        ModernMessageBox.MessageBoxType.Information, ModernMessageBox.MessageBoxButtons.OK, this);
                }
            }
            catch (BO.BlTemporaryNotAvailableException ex)
            {
                ModernMessageBox.Show(ex.Message, "Operation Not Available", 
                    ModernMessageBox.MessageBoxType.Warning, ModernMessageBox.MessageBoxButtons.OK, this);
            }
            catch (Exception ex)
            {
                ModernMessageBox.Show($"Error: {ex.Message}", "Error", 
                    ModernMessageBox.MessageBoxType.Error, ModernMessageBox.MessageBoxButtons.OK, this);
            }
        }

        #endregion
    }
}