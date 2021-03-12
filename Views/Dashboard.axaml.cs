using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using WorkStatus.ViewModels;

namespace WorkStatus.Views
{
    public class Dashboard : Window
    {
        public DashboardViewModel _dashboardVM;
        public Dashboard()
        {
            InitializeComponent();
            _dashboardVM = new DashboardViewModel();
            this.DataContext = _dashboardVM;
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
