using Inventory_monitor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Inventory_monitor.Controls
{
    class CustomDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            Resource resource = item as Resource;
            FrameworkElement element = container as FrameworkElement;

            if (resource.IsGroup)
            {
                return element.FindResource("group") as DataTemplate;
            }
            else
            {
                return element.FindResource("resource") as DataTemplate;
            }
        }
    }
}
