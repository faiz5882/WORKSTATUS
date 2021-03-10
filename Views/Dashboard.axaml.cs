using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace WorkStatus.Views
{
    public class Dashboard : Window
    {
        public Dashboard()
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
