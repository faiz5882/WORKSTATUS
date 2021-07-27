using Avalonia.Controls;
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
using Avalonia.Controls;
using MessageBoxAvaloniaEnums = MessageBox.Avalonia.Enums;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia;
using WorkStatus.Utility;
using System.Windows.Threading;

namespace WorkStatus.ViewModels
{
    public class ForgotPasswordViewModel : ReactiveObject, INotifyPropertyChanged//ReactiveObject
    {
        private Window _window;
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        int counter = 0;
        TextBlock tt;

        ThemeManager ThemeManager;
        public object UserLoginSqliteService;
        private ForgetPasswordResponseModel _forgotPasswordResponse;
        private string _baseURL = string.Empty;
        private readonly IAccounts _services;
        #region All ReactiveCommand
        public ReactiveCommand<Unit, Unit> CommandLogin { get; }
        public ReactiveCommand<Unit, Unit> CommandClosed { get; }


        #endregion
        #region constructor
        public ForgotPasswordViewModel(Window window)
        {
            _window = window;
            ThemeManager = new ThemeManager();
            _services = new AccountService();
            CommandLogin = ReactiveCommand.Create(ForgotPassword);
            CommandClosed = ReactiveCommand.Create(ClosedWindow);

            tt = _window.FindControl<TextBlock>("errorStatus");

            _forgotPasswordResponse = new ForgetPasswordResponseModel();
            StackPanelLogo = ThemeManager.StackPanelLogoColor;
            TxtWelcomeColor = ThemeManager.TxtWelcomeColor;
        }

        #endregion

        #region DataBinding Members

        private string errorMes;
        public string ErrorMes
        {
            get => errorMes;
            set
            {
                errorMes = value;
                RaisePropertyChanged("ErrorMes");
            }
        }

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
        public void ValidateFormsAndError(string strMessage, int timeOut)
        {
            DateTime oCurrentDate2 = DateTime.Now;
            int s = oCurrentDate2.Second;
            int result = s + timeOut;
            if (result >= 60)
            {
                s = 0;
                result = s + timeOut;
            }
            counter = result;
            ErrorMes = strMessage;
            tt.IsVisible = true;
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Start();
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            DateTime oCurrentDate = DateTime.Now;
            int s = oCurrentDate.Second;

            if (counter == s)
            {

                tt.IsVisible = false;
                dispatcherTimer.Stop();
            }
            //else
            //{
            //    oCurrentDate.AddSeconds(1);
            //}
        }
        private void ChangeLoginWindow()
        {
            try
            {
                if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var window = new Login();
                    var prevwindow = desktop.MainWindow;
                    desktop.MainWindow = window;
                    desktop.MainWindow.Show();
                    prevwindow.Close();
                    prevwindow = null;
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                LogFile.ErrorLog(ex);
            }
        }
        void ClosedWindow()
        {
            ChangeLoginWindow();
        }
        async void ForgotPassword()
        {
            try
            {


                if (string.IsNullOrEmpty(Email))
                {

                    ValidateFormsAndError("Email is required!", 5);
                    return;
                    //  var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager
                    //.GetMessageBoxStandardWindow("Error", "Email is required!", MessageBoxAvaloniaEnums.ButtonEnum.Ok);
                    //  var r = await messageBoxStandardWindow.ShowDialog(_window);
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

                            ValidateFormsAndError(_forgotPasswordResponse.Response.Message, 5);
                            return;
                            //var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager
                            // .GetMessageBoxStandardWindow("Success", _forgotPasswordResponse.Response.Message, MessageBoxAvaloniaEnums.ButtonEnum.Ok);
                            //var r = await messageBoxStandardWindow.ShowDialog(_window);
                        }
                        else
                        {
                            ValidateFormsAndError(_forgotPasswordResponse.Response.Message, 5);
                            return;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
                ValidateFormsAndError("Something went wrong!. Please try again.", 5);
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
