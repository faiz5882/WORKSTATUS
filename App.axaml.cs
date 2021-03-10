using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using WorkStatus.ViewModels;
using WorkStatus.Views;

namespace WorkStatus
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                //desktop.MainWindow = new Login
                //{
                //    DataContext = new MainWindowViewModel(),
                //};
                desktop.MainWindow = new ResetPassword();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
