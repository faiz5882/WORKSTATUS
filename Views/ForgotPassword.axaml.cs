using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using WorkStatus.ViewModels;

namespace WorkStatus.Views
{
    public class ForgotPassword : Window
    {
        private WindowNotificationManager _notificationArea;
        public ForgotPassword()
        {
            InitializeComponent();
            _notificationArea = new WindowNotificationManager(this)
            {
                Position = NotificationPosition.BottomLeft
            };
            DataContext = new ForgotPasswordViewModel(_notificationArea);
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
