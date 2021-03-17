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
using WorkStatus.Models.ReadDTO;
using WorkStatus.Models.WriteDTO;
using WorkStatus.Views;

namespace WorkStatus.ViewModels
{
    public class ForgotPasswordViewModel : ReactiveObject, INotifyPropertyChanged//ReactiveObject
    {
        ThemeManager ThemeManager;
        public object UserLoginSqliteService;
        private ForgetPasswordResponseModel _forgotPasswordResponse;
        private string _baseURL = string.Empty;
        private readonly IAccounts _services;
        #region All ReactiveCommand
        public ReactiveCommand<Unit, Unit> CommandLogin { get; }
        #endregion
        #region constructor
        public ForgotPasswordViewModel(IManagedNotificationManager notificationManager)
        {
            ThemeManager = new ThemeManager();
            _notificationManager = notificationManager;
            _services = new AccountService();
            CommandLogin = ReactiveCommand.Create(ForgotPassword);
            _forgotPasswordResponse = new ForgetPasswordResponseModel();
            StackPanelLogo = ThemeManager.StackPanelLogoColor;
            TxtWelcomeColor = ThemeManager.TxtWelcomeColor;
           
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

        #endregion

        #region Methods

        

        async void ForgotPassword()
        {
            if (string.IsNullOrEmpty(Email))
            {
                NotificationManager.Show(new Avalonia.Controls.Notifications.Notification("Error", "Email is required!", NotificationType.Error));
            }
            else
            {
                _baseURL = Configurations.UrlConstant + Configurations.ForgotPasswordApiConstant;
                ForgetPasswordReqeuestModel data = new ForgetPasswordReqeuestModel()
                {
                    email = Email
                };
                _forgotPasswordResponse = await _services.ForgotPasswordAsync(new Get_API_Url().ForgotPasswordApi(_baseURL), data);
                if (_forgotPasswordResponse != null)
                {
                    if (_forgotPasswordResponse.Response.Code == "200")
                    {
                        //MainWindow mainwindow = new MainWindow();
                       // await MessageBox.Show(mainwindow, "Success", _forgotPasswordResponse.Response.Message, MessageBox.MessageBoxButtons.YesNoCancel);
                        NotificationManager.Show(new Avalonia.Controls.Notifications.Notification("Success", _forgotPasswordResponse.Response.Message, NotificationType.Success));
                    }
                    else
                    {
                        NotificationManager.Show(new Avalonia.Controls.Notifications.Notification("Error", _forgotPasswordResponse.Response.Message, NotificationType.Error));
                    }
                }

            }
        }
        #endregion

        #region NotificationManager
        private IManagedNotificationManager _notificationManager;
        public IManagedNotificationManager NotificationManager
        {
            get { return _notificationManager; }
            set { this.RaiseAndSetIfChanged(ref _notificationManager, value); }
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
