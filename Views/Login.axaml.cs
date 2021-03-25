using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using WorkStatus.Interfaces;
using WorkStatus.ViewModels;

namespace WorkStatus.Views
{
    public class Login : Window
    {
        
        public Login()
        {
            InitializeComponent();
        
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
