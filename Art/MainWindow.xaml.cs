using HandyControl.Controls;
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

namespace Art
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : BlurWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void StackPanel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonConfig_Click(object sender, RoutedEventArgs e)
        {
            popupConfig.IsOpen = true;
        }

        private void CoverViewItem_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void CoverViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
