using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace WorkStatus.Views
{
    public class ResetPassword : Window
    {
        public ResetPassword()
        {
            InitializeComponent();
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
