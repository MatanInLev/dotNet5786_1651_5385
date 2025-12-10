using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace PL.Courier
{

    /// <summary>
    /// Interaction logic for CourierListWindow.xaml
    /// </summary>
    public partial class CourierListWindow : Window
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        // In a real app this should be the logged-in admin id.
        // For now 0 is fine as long as BL accepts it.
        private readonly int _userId = 0;

        /// <summary>
        /// The list bound to the DataGrid
        /// </summary>
        public IEnumerable<BO.CourierInList> CourierList
        {
            get => (IEnumerable<BO.CourierInList>)GetValue(CourierListProperty);
            set => SetValue(CourierListProperty, value);
        }

        public static readonly DependencyProperty CourierListProperty =
            DependencyProperty.Register(
                nameof(CourierList),
                typeof(IEnumerable<BO.CourierInList>),
                typeof(CourierListWindow),
                new PropertyMetadata(null));

        /// <summary>
        /// Filter criterion bound to the ComboBox SelectedValue
        /// (All / Active / Inactive)
        /// </summary>
        public BO.ActiveFilter Filter { get; set; } = BO.ActiveFilter.All;

        public CourierListWindow()
        {
            InitializeComponent();

            // initial load
            QueryCourierList();
            (s_bl.Courier as BlApi.IObservable)?.AddObserver(QueryCourierList);
        }

        /// <summary>
        /// Runs the BL query and updates CourierList according to the filter.
        /// </summary>
        private void QueryCourierList()
        {
            // map enum -> bool? for the BL method:
            // null = all, true = active, false = inactive
            bool? isActive = Filter switch
            {
                BO.ActiveFilter.All => null,
                BO.ActiveFilter.Active => true,
                BO.ActiveFilter.NotActive => false,
                _ => null
            };


            // This matches your ICourier:
            // IEnumerable<BO.CourierInList> GetList(int userId, bool? isActive = null, BO.Vehicle? vehicle = null);
            CourierList = s_bl.Courier.GetList(_userId, isActive);
        }

        /// <summary>
        /// Called whenever the ComboBox selection changes.
        /// Filter property is already updated by binding, we just re-query.
        /// </summary>
        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            QueryCourierList();
        }

        // OPTIONAL – only אם עשית observers

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            (s_bl.Courier as BlApi.IObservable)?.RemoveObserver(QueryCourierList);
        }

    }
}
