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
using System.Windows.Shapes;

namespace Inventory_monitor.Views
{
    /// <summary>
    /// Interaction logic for CommentWindow.xaml
    /// </summary>
    public partial class CommentWindow : Window
    {
        string initText;
        Transaction transaction;
        MainWindow mainWindow;
        public CommentWindow(Transaction transaction, string initText, MainWindow mainWindow)
        {
            InitializeComponent();
            this.transaction = transaction;
            this.initText = initText;
            this.mainWindow = mainWindow;
            DataContext = initText;
        }

        private void SaveComment_Click(object sender, RoutedEventArgs e)
        {
            if (initText != commentText.Text)
                mainWindow.EditTransaction(transaction, commentText.Text);
            Close();
        }
    }
}
