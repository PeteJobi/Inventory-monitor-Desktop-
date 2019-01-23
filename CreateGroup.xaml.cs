using Inventory_monitor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Inventory_monitor.Views
{
    /// <summary>
    /// Interaction logic for CreateGroup.xaml
    /// </summary>
    public partial class CreateGroup : Window
    {
        MainWindow mainWindow;
        public CreateGroup(Resource resource, MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            groupInfo.Text = resource.Title + " will be moved to this group";
            groupNameText.Focus();
        }
        public CreateGroup(ObservableCollection<Resource> selected, MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            groupInfo.Text = selected[0].Title + " and " + (selected.Count - 1) + " others will be moved to this group";
            groupNameText.Focus();
        }

        private void CreateGroup_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.CreateGroup(groupNameText.Text, descriptionText.Text);
            Close();
        }
    }
}
