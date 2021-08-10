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
using Avalonia;
using Avalonia.Threading;
using Window = Avalonia.Controls.Window;
using Application = Avalonia.Application;
using AvaloniaProgressRing;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WorkStatus.ViewModels
{
    public class LoginViewModel : ReactiveObject, INotifyPropertyChanged//ReactiveObject
    {
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        int counter = 0;
        ThemeManager ThemeManager;
        private Avalonia.Controls.Window _Window;
        TextBlock tt;
        private LoginRequestDTOEntity loginRequestDTO;
        public object UserLoginSqliteService;
        private LoginResponse _loginResponse;
        private string _baseURL = string.Empty;
        private readonly IAccounts _services;
        bool isWindows = false;
        bool isMac = false;
        public ProgressRing LoginLoder;
        public LoginDTOEntity _loginEntity { get; set; }
        private tbl_UserDetails user;
        #region All ReactiveCommand
        public ReactiveCommand<Unit, Unit> CommandLogin { get; }
        public ReactiveCommand<Unit, Unit> CommandForgotPassword { get; }
        public ReactiveCommand<Unit, Unit> CommandSignUp { get; }
        #endregion
        #region constructor       
        public LoginViewModel(Window window)
        {
            _Window = window;
            ThemeManager = new ThemeManager();
            _services = new AccountService();
            CommandLogin = ReactiveCommand.Create(Login);
            CommandSignUp = ReactiveCommand.Create(SignUp);
            CommandForgotPassword = ReactiveCommand.Create(NavigateToForgotPasswordScreen);
            _loginResponse = new LoginResponse();
            StackPanelLogo = ThemeManager.NewStackPanelLogoColor;
            TxtWelcomeColor = ThemeManager.TxtWelcomeColor;
            tt = _Window.FindControl<TextBlock>("errorStatus");
            LoginLoder = _Window.FindControl<ProgressRing>("loginloder");

            LoginLoder.IsVisible = false;
            IsLoginEnable = true;
            //BuildConnectionString();

            //email = "sonamsoftdev@yopmail.com";
           // password = "sonam@123";
            //email = "ownerdevy@yopmail.com";
            //password = "Test@12345";
            isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
            isMac = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX);


        }

        #endregion
        #region Methods  
        public void SignUp()
        {
            var url = Configuration.Configurations.SignUpUrl;
            NavigateToBrowser(url);
        }
        private void NavigateToBrowser(string url)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? url : "open",
                CreateNoWindow = true,
                UseShellExecute = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            });
        }
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
        private void ChangeDashBoardWindow()
        {
            try
            {
                if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var window = new Dashboard();
                    var prevwindow = desktop.MainWindow;
                    desktop.MainWindow = window;
                    desktop.MainWindow.Show();
                    prevwindow.Close();
                    prevwindow = null;
                }
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
        }
        private void ChangetoForgotPasswordWindow()
        {
            try
            {
                if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var window = new ForgotPassword();
                    var prevwindow = desktop.MainWindow;
                    desktop.MainWindow = window;
                    desktop.MainWindow.Show();
                    prevwindow.Close();
                    prevwindow = null;
                }
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
        }

        async void Login()
        {
            try
            {
               

                if (string.IsNullOrEmpty(Email))
                {
                    LoginLoder.IsVisible = false;
                    IsLoginEnable = true;
                    ValidateFormsAndError("Email is required", 5);                   

                    return;
                }
                else if (string.IsNullOrEmpty(Password))
                {
                    LoginLoder.IsVisible = false;
                    IsLoginEnable = true;
                    ValidateFormsAndError("Password is required", 5);
                    return;
                }
                else
                {
                    LoginLoder.IsVisible = true;
                    IsLoginEnable = false;
                    string deviceType = "";

                    loginRequestDTO = new LoginRequestDTOEntity()
                    {
                        deviceId = Environment.MachineName.ToStrVal(),
                        deviceType = isWindows == true ? "Windows" : "Mac",
                        email = Email,
                        password = Password,
                    };
                    _baseURL = Configurations.UrlConstant + Configurations.LoginApiConstant;
                    _loginResponse = await _services.LoginAsync(new Get_API_Url().LoginApi(_baseURL, Email, Password), loginRequestDTO);
                    if (_loginResponse != null)
                    {
                        if (_loginResponse.Response.Code == "200")
                        {


                            BaseService<tbl_UserDetails> dbService = new BaseService<tbl_UserDetails>();
                            dbService.Delete(new tbl_UserDetails());
                            // BaseService<tbl_IdleTimeDetails> dbService2= new BaseService<tbl_IdleTimeDetails>();
                            // dbService2.Delete(new tbl_IdleTimeDetails());

                            Common.Storage.TokenId = _loginResponse.Response.Data.Token;
                            Common.Storage.ServerOrg_Id = _loginResponse.Response.Data.Org_Id;
                            Common.Storage.ServerSd_Token = _loginResponse.Response.Data.Sd_Token;
                            Common.Storage.LoginId = _loginResponse.Response.Data.Email;
                            Common.Storage.LoginUserID = _loginResponse.Response.Data.Id.ToStrVal();
                            user = new tbl_UserDetails()
                            {
                                UserId = _loginResponse.Response.Data.Id,
                                UserEmail = _loginResponse.Response.Data.Email,
                                UserName = _loginResponse.Response.Data.Name,
                                Token = _loginResponse.Response.Data.Token,
                                CreatedOn = Convert.ToString(DateTime.Now),
                                UpdatedOn = Convert.ToString(DateTime.Now),
                                IsRemember = "true",
                                ServerToken = _loginResponse.Response.Data.Sd_Token,
                                OrganisationId = _loginResponse.Response.Data.Org_Id.ToInt32()
                            };

                            //tbl_IdleTimeDetails tbl_Idle = new tbl_IdleTimeDetails() {
                            //    Sno = 0,
                            //    IsActive = 0,
                            //    ProjectId = 2,
                            //    UserId = 19,
                            //    CreatedOn =Convert.ToString(DateTime.Now),
                            //    ProjectIdleTime=""
                            //};

                            //   BaseService<tbl_IdleTimeDetails> dbService4 = new BaseService<tbl_IdleTimeDetails>();
                            // dbService4.Add(tbl_Idle);


                            long userID = new UserLoginSqliteService().InsertUser(user);
                            if (userID > 0)
                            {
                                LoginLoder.IsVisible = false;
                                IsLoginEnable = true;

                                // BaseService<tbl_Temp_SyncTimer> service2 = new BaseService<tbl_Temp_SyncTimer>();
                                //service2.Delete(new tbl_Temp_SyncTimer());
                                //BaseService<tbl_TempSyncTimerTodoDetails> service3 = new BaseService<tbl_TempSyncTimerTodoDetails>();
                                //service3.Delete(new tbl_TempSyncTimerTodoDetails());

                                ChangeDashBoardWindow();
                            }
                        }
                        else
                        {
                            LoginLoder.IsVisible = false;
                            IsLoginEnable = true;
                            _loginResponse.Response.Message = _loginResponse.Response.Message.Contains("Must be a valid email.") ? _loginResponse.Response.Message.Replace("Must be a valid email.", "Please enter valid email.") : _loginResponse.Response.Message;
                            ValidateFormsAndError(_loginResponse.Response.Message, 5);
                            return;
                        }
                    }
                    else
                    {
                        LoginLoder.IsVisible = false;
                        IsLoginEnable = true;
                        ValidateFormsAndError("Something went wrong!. Please try again.", 5);
                        return;                       
                    }
                }
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
                LoginLoder.IsVisible = false;
                IsLoginEnable = true;
                ValidateFormsAndError("Something went wrong!. Please try again.", 5);
            }
        }

        async void NavigateToForgotPasswordScreen()
        {
            ChangetoForgotPasswordWindow();
        }
        #endregion

        #region DataBinding Members
        private bool _isLoginEnable;
        public bool IsLoginEnable
        {
            get => _isLoginEnable;
            set
            {
                _isLoginEnable = value;
                RaisePropertyChanged("IsLoginEnable");
            }
        }
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
