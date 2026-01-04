using BO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace PL.Courier
{
    public partial class FinishDeliveryDialog : Window
    {
        public IEnumerable<DeliveryStatus> StatusOptions { get; } = Enum.GetValues(typeof(DeliveryStatus)).Cast<DeliveryStatus>();

        public DeliveryStatus SelectedStatus { get; set; } = DeliveryStatus.Delivered;

        public FinishDeliveryDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Validate_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
