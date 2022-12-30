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

namespace WpfSysUs.Views
{
    /// <summary>
    /// Interaction logic for WindowReport.xaml
    /// </summary>
    public partial class WindowReport : Window
    {
        public WindowReport()
        {
            InitializeComponent();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
          Properties.Settings.Default.Save();
        }
    }
}
