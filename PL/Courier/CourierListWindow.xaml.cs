using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PL.Courier;

/// <summary>
/// Interaction logic for CourierListWindow.xaml
/// </summary>
public partial class CourierListWindow : Window
{
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();



    public IEnumerable<BO.CourierInList> CourierList
    {
        get { return (IEnumerable<BO.CourierInList>)GetValue(CourierInListsProperty); }
        set { SetValue(CourierInListsProperty, value); }
    }

    // Using a DependencyProperty as the backing store for CourierInLists.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty CourierInListsProperty =
        DependencyProperty.Register(nameof(CourierInLists), typeof(IEnumerable<BO.CourierInList>), typeof(CourierListWindow), new PropertyMetadata(0));


    public CourierListWindow()
    {
        InitializeComponent();
    }
}
