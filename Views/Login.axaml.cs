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
        private WindowNotificationManager _notificationArea;
       
        // public LoginViewModel _loginVM;
        public Login()
        {
            InitializeComponent();
           // _loginVM = new LoginViewModel();
          //  this.DataContext = _loginVM;
            _notificationArea = new WindowNotificationManager(this)
            {
                Position = NotificationPosition.BottomLeft                              
            };
            DataContext = new LoginViewModel(_notificationArea);            

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
