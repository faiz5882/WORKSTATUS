using Avalonia.Controls.Notifications;
using Avalonia.Media;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using WorkStatus.APIServices;
using WorkStatus.Configuration;
using WorkStatus.Interfaces;
using WorkStatus.Models;
using System.Windows;
using WorkStatus.Models.WriteDTO;
using WorkStatus.Models.ReadDTO;
using WorkStatus.Common;
using WorkStatus.Utility;
using System.Configuration;
using WorkStatus.Views;
using Avalonia.Controls.ApplicationLifetimes;

namespace WorkStatus.ViewModels
{
    public class LoginViewModel : ReactiveObject, INotifyPropertyChanged//ReactiveObject
    {
        ThemeManager ThemeManager;
        public object UserLoginSqliteService;
        private LoginResponse _loginResponse;
        private string _baseURL = string.Empty;
        private readonly IAccounts _services;
        public LoginDTOEntity _loginEntity { get; set; }
        #region All ReactiveCommand
        public ReactiveCommand<Unit, Unit> CommandLogin { get; }
        #endregion
        #region constructor       
        public LoginViewModel(IManagedNotificationManager notificationManager)
        {
            ThemeManager = new ThemeManager();
            _notificationManager = notificationManager;
            _services = new AccountService();
            CommandLogin = ReactiveCommand.Create(Login);
            _loginResponse = new LoginResponse();                                 
            StackPanelLogo = ThemeManager.StackPanelLogoColor;
            TxtWelcomeColor = ThemeManager.TxtWelcomeColor;
            BuildConnectionString();
        }

        #endregion
        #region Methods  

        public void BuildConnectionString()
        {
            if (String.IsNullOrEmpty(Storage.ConnectionString))
            {
                Storage.ConnectionString =Configuration.Configurations.GetConnectionString();
            }
        }
        
        async void Login()
        {
            
            if(string.IsNullOrEmpty(Email))
            {
                
                 NotificationManager.Show(new Avalonia.Controls.Notifications.Notification("Error", "Email is required!", NotificationType.Error));
                // MessageBox.Show("This program is designed to be run from Counterpoint.  For information about how to install it into " +
                //"an instance of Counterpoint, see Readme file provided along with this program.", "C&K Admissions");
            }
            else if(string.IsNullOrEmpty(Password))
            {
                NotificationManager.Show(new Avalonia.Controls.Notifications.Notification("Error", "Password is required!", NotificationType.Error));
            }
            else
            {

                //var dialog = new Dashboard();
                //var mainWindow = (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
                //await dialog.ShowDialog(mainWindow);

                _baseURL = Configurations.UrlConstant + Configurations.LoginApiConstant;
                _loginResponse = await _services.LoginAsync(new Get_API_Url().LoginApi(_baseURL, Email, Password), _loginEntity);
                if (_loginResponse != null)
                {
                    if (_loginResponse.Response.Code == "200")
                    {
                        tbl_UserDetails user = new tbl_UserDetails()
                        {
                            UserId = _loginResponse.Response.Data.Id,
                            UserEmail = _loginResponse.Response.Data.Email,
                            UserName = _loginResponse.Response.Data.Name,
                            Token = _loginResponse.Response.Data.Token,
                            CreatedOn = Convert.ToString(DateTime.Now),
                            UpdatedOn = Convert.ToString(DateTime.Now),
                            IsRemember = "true"
                        };
                        long userID = new UserLoginSqliteService().InsertUser(user);
                        if (userID > 0)
                        {
                            var dialog = new Dashboard();
                            var mainWindow = (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
                            await dialog.ShowDialog(mainWindow);
                        }
                    }
                }

            }
        }

        //public long InsertUser(tbl_UserDetails tbl_User)
        //{
        //    return new UserLoginSqliteService().Add(tbl_User);
        //}
        #endregion
        #region NotificationManager
        private IManagedNotificationManager _notificationManager;
        public IManagedNotificationManager NotificationManager
        {
            get { return _notificationManager; }
            set { this.RaiseAndSetIfChanged(ref _notificationManager, value); }
        }
        #endregion
        #region DataBinding Members

        private SolidColorBrush _stackPanelLogo;
        public SolidColorBrush StackPanelLogo
        {
            get => _stackPanelLogo;
            set
            {
                _stackPanelLogo = value;
                RaisePropertyChanged("StackPanelLogo");
            }
        }
        private SolidColorBrush _txtWelcomeColor;
        public SolidColorBrush TxtWelcomeColor
        {
            get => _txtWelcomeColor;
            set
            {
                _txtWelcomeColor = value;
                RaisePropertyChanged("TxtWelcomeColor");
            }
        }
        private string email;
        public string Email
        {
            get => email;
            set
            {
                email = value;
                RaisePropertyChanged("Email");
            }
        }

        private string password;
        public string Password
        {
            get => password;
            set
            {
                password = value;
                RaisePropertyChanged("Password");
            }
        }
        #endregion
        #region MVVM INotifyPropertyChanged     
        public event PropertyChangedEventHandler? PropertyChanged;
        public void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
