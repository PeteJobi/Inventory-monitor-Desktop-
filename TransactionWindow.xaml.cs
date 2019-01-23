using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for TransactionWindow.xaml
    /// </summary>
    public partial class TransactionWindow : Window
    {
        bool isRemoval;
        int max;
        MainWindow mainWindow;
        public TransactionWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            isRemoval = false;
            this.mainWindow = mainWindow;
            numError.Visibility = Visibility.Collapsed;
            transNumText.Focus();
        }
        public TransactionWindow(int max, MainWindow mainWindow)
        {
            InitializeComponent();
            isRemoval = true; ;
            this.mainWindow = mainWindow;
            this.max = max;
            numError.Visibility = Visibility.Collapsed;
            transNumText.Focus();
        }

        private void SaveTransaction(object sender, RoutedEventArgs e)
        {
            Regex regex = new Regex(@"(\D+)");
            if (string.IsNullOrEmpty(transNumText.Text) || regex.IsMatch(transNumText.Text)) {
                numError.Visibility = Visibility.Visible;
                return;
            }
            if (isRemoval && int.Parse(transNumText.Text) > max)
            {
                MessageBox.Show("You cannot remove more than " + max + " items from this resource.");
                return;
            }
            mainWindow.AddTransaction(int.Parse(transNumText.Text), isRemoval, commentText.Text);
            Close();
        }

        private void TransNumText_TextChanged(object sender, TextChangedEventArgs e)
        {
            numError.Visibility = Visibility.Collapsed;
        }
    }
}
