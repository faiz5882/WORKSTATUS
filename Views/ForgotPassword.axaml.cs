using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using WorkStatus.ViewModels;

namespace WorkStatus.Views
{
    public class ForgotPassword : Window
    {
        
        public ForgotPassword()
        {
            InitializeComponent();            
            this.DataContext = new ForgotPasswordViewModel(this);
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
