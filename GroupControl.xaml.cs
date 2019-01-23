using Inventory_monitor.Models;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Inventory_monitor.MyUserControls
{
    /// <summary>
    /// Interaction logic for GroupControl.xaml
    /// </summary>
    public partial class GroupControl : UserControl
    {
        public GroupControl()
        {
            InitializeComponent();
        }

        private void GroupBut_Click(object sender, RoutedEventArgs e)
        {
            Resource resource = (sender as Button).DataContext as Resource;
            resource.GetMainWindow().Select(resource);
        }

        private void GoBut_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Resource resource = (sender as Button).DataContext as Resource;
            resource.GetMainWindow().GoToGroup(resource);
        }
    }
}
