using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LangLang
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void ComboBoxItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = (ComboBoxItem)sender;
            var combo = ItemsControl.ItemsControlFromItemContainer(item) as ComboBox;
            if (combo == null) return;

            var clickedDataItem = combo.ItemContainerGenerator.ItemFromContainer(item);

            if (Equals(combo.SelectedItem, clickedDataItem))
            {
                combo.SelectedIndex = -1;
                combo.IsDropDownOpen = false;
                e.Handled = true;
            }
        }
    }
}
