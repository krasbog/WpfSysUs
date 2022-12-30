using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WpfSysUs.Views;
using WpfSysUs.Models;
using WpfSysUs.ViewModels;
using System.Collections.ObjectModel;

namespace WpfSysUs
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [Obsolete]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
        protected override void OnStartup(StartupEventArgs e)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
        {
            base.OnStartup(e);
            
            var strings = new ObservableCollection<string>();
            var systemUsers = new ObservableCollection<SystemUser>();
            var errorStrings = new ObservableCollection<string>();
            var model = new SystemUserModel(strings, systemUsers, errorStrings);
            var systemUserPresenter = new SystemUserPresenter(model, systemUsers);
            var mainWindow = new MainWindow { DataContext = systemUserPresenter };

          

            mainWindow.Show();
        }
    }
}
