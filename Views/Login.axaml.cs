using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using WorkStatus.Interfaces;
using WorkStatus.ViewModels;
using System.Windows;
using System.Windows.Media.Animation;
using Window = Avalonia.Controls.Window;
using Application = Avalonia.Application;
namespace WorkStatus.Views
{
    public class Login : Window
    {
        
        public Login()
        {
            InitializeComponent();
            
           // Storyboard sb = Resources["sbHideAnimation"] as Storyboard;
           // sb.Begin("");


            //_notificationArea = new WindowNotificationManager(this)
            //{
            //    Position = NotificationPosition.BottomLeft                              
            //};
            this.DataContext = new LoginViewModel(this);            

#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
