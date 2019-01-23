using Inventory_monitor.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace Inventory_monitor.Views
{
    /// <summary>
    /// Interaction logic for AddResourceWindow.xaml
    /// </summary>
    public partial class AddResourceWindow : Window
    {
        MainWindow mainWindow;
        string imagePath;
        public AddResourceWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            removeImage.IsEnabled = false;
            resourceNameText.Focus();
            nameError.Visibility = Visibility.Collapsed;
            quantityError.Visibility = Visibility.Collapsed;
            quantityText.TextChanged += QuantityText_TextChanged;
        }

        private void ViewImage(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(imagePath)) return;
            ViewPicture viewPicture = new ViewPicture();
            viewPicture.DataContext = imagePath;
            viewPicture.Show();
        }

        private void SaveResource(object sender, RoutedEventArgs e)
        {
            if (imagePath != null)
            {
                string name = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond).ToString() + Path.GetExtension(imagePath);
                string dest = MainWindow.dataFolder + name;
                File.Copy(imagePath, dest);
                imagePath = dest;
            }

            if (string.IsNullOrWhiteSpace(resourceNameText.Text)) nameError.Visibility = Visibility.Visible;
            Regex regex = new Regex(@"(\D+)");
            if(string.IsNullOrWhiteSpace(quantityText.Text) || regex.IsMatch(quantityText.Text))
                quantityError.Visibility = Visibility.Visible;
            if (string.IsNullOrWhiteSpace(resourceNameText.Text) || string.IsNullOrWhiteSpace(quantityText.Text) || regex.IsMatch(quantityText.Text))
                return;
            mainWindow.AddResource(resourceNameText.Text, descriptionText.Text, int.Parse(quantityText.Text), imagePath);
            Close();
        }

        private void PickImage(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select picture";
            ofd.Filter = "Image files (*.png, *.jpg)|*.png;*.jpg";
            bool? success = ofd.ShowDialog();
            if (success == true)
            {
                imagePath = ofd.FileName;
                image.Source = new BitmapImage(new Uri(imagePath));
                removeImage.IsEnabled = true;
            }
            else
            {
                MessageBox.Show("Failed to open File Dialog");
            }
        }

        private void RemoveImage(object sender, RoutedEventArgs e)
        {
            imagePath = null;
            image.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(Resource.RES_DEFAULT_IMAGE)));
            removeImage.IsEnabled = false;
        }

        private void ResourceNameText_TextChanged(object sender, TextChangedEventArgs e)
        {
            nameError.Visibility = Visibility.Collapsed;
        }
        private void QuantityText_TextChanged(object sender, TextChangedEventArgs e)
        {
            quantityError.Visibility = Visibility.Collapsed;
        }
    }
}
