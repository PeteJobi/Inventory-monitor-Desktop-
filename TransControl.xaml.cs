using Inventory_monitor.Models;
using Inventory_monitor.Views;
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
    /// Interaction logic for TransControl.xaml
    /// </summary>
    public partial class TransControl : UserControl
    {
        public TransControl()
        {
            InitializeComponent();
        }

        private void EditBut_Click(object sender, RoutedEventArgs e)
        {
            Transaction transaction = (Transaction) (sender as Button).Tag;
            CommentWindow commentWindow = new CommentWindow(transaction, transaction.Comment, transaction.Parent.GetMainWindow());
            commentWindow.ShowDialog();
        }

        private void DelBut_Click(object sender, RoutedEventArgs e)
        {
            Transaction transaction = (Transaction)(sender as Button).Tag;
            transaction.Parent.GetMainWindow().DeleteTransactions(transaction);
        }
    }
}
