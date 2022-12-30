using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfSysUs.ViewModels.MVVM;
using WpfSysUs.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using System.Reflection;
using System.Windows;
using System.IO;


namespace WpfSysUs.ViewModels
{
    class SystemUserPresenter : Presenter
    {
        private readonly SystemUserModel _model;
        private string _filePath = string.Empty;
        private bool _isPeriod = false;
        private DateTime _dateTimeFrom = DateTime.Parse("01/04/17");
        private DateTime _dateTimeTo = DateTime.Parse("01/04/17");
        private bool _param1 = false;
        private bool _param2 = false;
        private bool _param3 = false;
        private bool _param4 = false;
        private bool _param5 = false;

        public string FilePath
        {
            get => _filePath;
            set
            {
                Properties.Settings.Default.FilePath = value;
                Update(ref _filePath, value);
            }
        }

        public bool IsPeriod
        {
            get => _isPeriod;
            set
            {
                Properties.Settings.Default.IsPeriod = value;
                Update(ref _isPeriod, value);
            }
        }

        public DateTime DateTimeFrom
        {
            get => _dateTimeFrom;
            set
            {
                Properties.Settings.Default.DateTimeFrom = value;
                Update(ref _dateTimeFrom, value);
            }
        }

        public DateTime DateTimeTo
        {
            get => _dateTimeTo;
            set
            {
                Properties.Settings.Default.DateTimeTo = value;
                Update(ref _dateTimeTo, value);
            }
        }

        public bool Param1
        {
            get => _param1;
            set
            {
                Properties.Settings.Default.Param1 = value;
                Update(ref _param1, value);
            }
        }

        public bool Param2
        {
            get => _param2;
            set
            {
                Properties.Settings.Default.Param2 = value;
                Update(ref _param2, value);
            }
        }

        public bool Param3
        {
            get => _param3;
            set
            {
                Properties.Settings.Default.Param3 = value;
                Update(ref _param3, value);
            }
        }

        public bool Param4
        {
            get => _param4;
            set
            {
                Properties.Settings.Default.Param4 = value;
                Update(ref _param4, value);
            }
        }

        public bool Param5
        {
            get => _param5;
            set
            {
                Properties.Settings.Default.Param5 = value;
                Update(ref _param5, value);
            }
        }


        public SystemUserPresenter(SystemUserModel model, ObservableCollection<SystemUser> systemUsers)
        {
            _model = model;
            SystemUsers = systemUsers;
            this.SystemUsersView.Filter=new Predicate<object>(p => this.Filter(p as SystemUser));
            _filePath = Properties.Settings.Default.FilePath;
            _isPeriod = Properties.Settings.Default.IsPeriod;
            _dateTimeFrom = Properties.Settings.Default.DateTimeFrom;
            _dateTimeTo = Properties.Settings.Default.DateTimeTo;
            _param1 = Properties.Settings.Default.Param1;
            _param2 = Properties.Settings.Default.Param2;
            _param3 = Properties.Settings.Default.Param3;
            _param4 = Properties.Settings.Default.Param4;
            _param5 = Properties.Settings.Default.Param5;



        }
        
        void GetFilePath()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.FileName = "Report"; // Default file name
            dialog.DefaultExt = ".xls"; // Default file extension
            dialog.Filter = "Excel Worksheets|*.xls" +
                "|XML Documents|*.xml"; // Filter files by extension

           
            bool? result = dialog.ShowDialog();

            
            if (result == true)
            {
                
                FilePath = dialog.FileName;
               // if (dialog.CheckFileExists) File.Delete(FilePath);
            }


        }

         void ShowWindow (object parameter)
        {
            if (parameter == null)
                throw new ArgumentNullException("TargetWindowType");

            //Get the type.
            TypeInfo p = (TypeInfo)parameter;
            Type t = parameter.GetType();

            //Make sure the parameter passed in is a window.
            if (p.BaseType != typeof(Window))
                throw new InvalidOperationException("parameter is not a Window type");

            //Create the window.
            Window wnd = Activator.CreateInstance(p) as Window;
            wnd.DataContext = this;
            wnd.Show();
        }
        public ICommand ShowWindowReportCommand => new Command(parameter => ShowWindow(parameter));
        public ICommand GetFilePathCommand => new Command(_ => GetFilePath());
        public ICommand MakeReportCommand => new Command(_ => _model.MakeReport(
            _filePath,
            _isPeriod,
            _dateTimeFrom,
            _dateTimeTo,
            _param1,
            _param2,
            _param3,
            _param4,
            _param5));



        public ObservableCollection<SystemUser> SystemUsers { get;}
        public ICollectionView SystemUsersView
        {
            get { return CollectionViewSource.GetDefaultView(SystemUsers); }
        }
        private string _searchCriteria;
        public string SearchCriteria
        {
            get => _searchCriteria;
            set
            {
                base.Update(ref _searchCriteria, value); // use the base class to notify of the change to the property
                this.SystemUsersView.Refresh(); // call the refresh method
            }
        }
        private bool Filter(SystemUser systemUser)
        {
            string id = systemUser.ID.ToString();
            return this.SearchCriteria == null
                || id.IndexOf(this.SearchCriteria, StringComparison.OrdinalIgnoreCase) != -1
                || systemUser.Name.IndexOf(this.SearchCriteria, StringComparison.OrdinalIgnoreCase) != -1
                || systemUser.Organization.IndexOf(this.SearchCriteria, StringComparison.OrdinalIgnoreCase) != -1;
        }



    }
}
