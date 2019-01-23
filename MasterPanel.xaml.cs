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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Inventory_monitor.Views
{
    /// <summary>
    /// Interaction logic for MasterPanel.xaml
    /// </summary>
    public partial class MasterPanel : Page
    {
        MainWindow mainWindow;
        public Resource parent;
        public ObservableCollection<Resource> resources;
        public Resource lastSelected;
        public MasterPanel(MainWindow mainWindow, Resource parent)
        {
            this.mainWindow = mainWindow;
            this.parent = parent;
            resources = parent.Resources;
            InitializeComponent();
            resListBox.ItemsSource = parent.Resources;
            Initialize();
        }
        public MasterPanel(MainWindow mainWindow, ObservableCollection<Resource> resources)
        {
            this.mainWindow = mainWindow;
            this.resources = resources;
            InitializeComponent();
            resListBox.ItemsSource = resources;
            Initialize();
        }

        void Initialize()
        {
            
        }
    }
}
