using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using WorkStatus.Interfaces;
using WorkStatus.ViewModels;
using System.Windows;
using Window = Avalonia.Controls.Window;
using Application = Avalonia.Application;
using System.ComponentModel;
using System;
using Avalonia.Platform;
using System.Drawing;
using System.Reflection;

namespace WorkStatus.Views
{
    public class Login : Window
    {
        //private System.Windows.Forms.NotifyIcon m_notifyIcon;
        public Login()
        {
            InitializeComponent();
            //m_notifyIcon = new System.Windows.Forms.NotifyIcon();
            //m_notifyIcon.BalloonTipText = "WorkStatus.";
            //m_notifyIcon.BalloonTipTitle = "WorkStatus";
            //m_notifyIcon.Text = "WorkStatus Active";
            //m_notifyIcon.Icon = LoaNnotifyIconResources("/Assets/LogoSmall.ico");            
            //m_notifyIcon.Click += new EventHandler(m_notifyIcon_Click);          
            //ShowTrayIcon(true);            
            this.DataContext = new LoginViewModel(this);            

#if DEBUG
            this.AttachDevTools();
#endif
        }
        public new System.Drawing.Icon LoaNnotifyIconResources(string localFilePath)
        {
            try
            {
                string assemblyName = Assembly.GetEntryAssembly().GetName().Name;
                var rawUri = localFilePath;
                var uri = new Uri($"avares://{assemblyName}{rawUri}");

                var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                var asset = assets.Open(uri);

                return new System.Drawing.Icon(asset);
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
            return null;
        }
        //void OnClose(object sender, CancelEventArgs args)
        //{
        //    m_notifyIcon.Dispose();
        //    m_notifyIcon = null;
        //}
        //private Avalonia.Controls.WindowState m_storedWindowState = Avalonia.Controls.WindowState.Normal;
        //void OnStateChanged(object sender, EventArgs args)
        //{
        //    if (WindowState == Avalonia.Controls.WindowState.Minimized)
        //    {
        //        Hide();
        //        if (m_notifyIcon != null)
        //            m_notifyIcon.ShowBalloonTip(2000);
        //    }
        //    else
        //        m_storedWindowState = WindowState;
        //}
        //void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs args)
        //{
        //    CheckTrayIcon();
        //}

        //void m_notifyIcon_Click(object sender, EventArgs e)
        //{
        //    Show();
        //    WindowState = m_storedWindowState;
        //}
        //void CheckTrayIcon()
        //{
        //    ShowTrayIcon(!IsVisible);
        //}

        //void ShowTrayIcon(bool show)
        //{
        //    if (m_notifyIcon != null)
        //        m_notifyIcon.Visible = show;
        //}
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
