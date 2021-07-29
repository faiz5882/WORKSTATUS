using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Threading;
using WorkStatus.Models;
using WorkStatus.Utility;
using WorkStatus.ViewModels;
using WorkStatus.Views;

namespace WorkStatus
{
    public class App : Application
    {
        static System.Threading.Mutex singleton = new Mutex(true, "WorkStatus");
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            //[WindowsVolume][Manufacturer]\[ProductName]
            //if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            //{

            //    desktop.MainWindow = new CMessageBox();
            //}
          

            CheckSingleton();
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {

                List<tbl_UserDetails> userList = new List<tbl_UserDetails>();
                BaseService<tbl_UserDetails> dbService = new BaseService<tbl_UserDetails>();
                userList = new List<tbl_UserDetails>(dbService.GetAll());
                if (userList != null && userList.Count > 0)
                {
                    Common.Storage.TokenId = userList[0].Token;
                    Common.Storage.ServerOrg_Id = userList[0].OrganisationId.ToStrVal();
                    Common.Storage.ServerSd_Token = userList[0].ServerToken;
                    Common.Storage.LoginId = userList[0].UserEmail;
                    Common.Storage.LoginUserID = userList[0].UserId.ToStrVal();
                    var window = new Dashboard();
                    desktop.MainWindow = window;
                }
                else
                {
                    var window = new Login();
                    desktop.MainWindow = window;
                }
            }
            base.OnFrameworkInitializationCompleted();
        }
        public void CheckSingleton()
        {
            if (!singleton.WaitOne(TimeSpan.Zero, true))
            {
                //there is already another instance running!
                Environment.Exit(0);
            }
        }
    }
}
