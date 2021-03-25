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
using WorkStatus.Common;
using Avalonia.Controls;
using MessageBoxAvaloniaEnums = MessageBox.Avalonia.Enums;

namespace WorkStatus.ViewModels
{
    public class LoginViewModel : ReactiveObject, INotifyPropertyChanged//ReactiveObject
    {
        ThemeManager ThemeManager;
       private Window _Window;
        public object UserLoginSqliteService;
        private LoginResponse _loginResponse;
        private string _baseURL = string.Empty;
        private readonly IAccounts _services;
        public LoginDTOEntity _loginEntity { get; set; }
        private tbl_UserDetails user;
        #region All ReactiveCommand
        public ReactiveCommand<Unit, Unit> CommandLogin { get; }
        public ReactiveCommand<Unit, Unit> CommandForgotPassword { get; }
        #endregion
        #region constructor       
        public LoginViewModel(Window window)
        {
            _Window = window;
            ThemeManager = new ThemeManager();          
            _services = new AccountService();
            CommandLogin = ReactiveCommand.Create(Login);
            CommandForgotPassword = ReactiveCommand.Create(NavigateToForgotPasswordScreen);
            _loginResponse = new LoginResponse();                                 
            StackPanelLogo = ThemeManager.StackPanelLogoColor;
            TxtWelcomeColor = ThemeManager.TxtWelcomeColor;
            //BuildConnectionString();
        }

        #endregion
        #region Methods  

       
        
        async void Login()
        {
            
            if(string.IsNullOrEmpty(Email))
            {

               
                var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager
               .GetMessageBoxStandardWindow("Error", "Email is required!", MessageBoxAvaloniaEnums.ButtonEnum.Ok);
                var r = await messageBoxStandardWindow.ShowDialog(_Window);
            }
            else if(string.IsNullOrEmpty(Password))
            {
                var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager
              .GetMessageBoxStandardWindow("Error", "Password is required!", MessageBoxAvaloniaEnums.ButtonEnum.Ok);
                var r = await messageBoxStandardWindow.ShowDialog(_Window);
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
                        BaseService<tbl_UserDetails> dbService = new BaseService<tbl_UserDetails>();
                        dbService.Delete(new tbl_UserDetails());
                        Common.Storage.TokenId = _loginResponse.Response.Data.Token;
                         user = new tbl_UserDetails()
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

        async void NavigateToForgotPasswordScreen()
        {
            var dialog = new ForgotPassword();
            var mainWindow = (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            await dialog.ShowDialog(mainWindow);            
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
