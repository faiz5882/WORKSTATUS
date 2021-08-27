using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using WorkStatus.ViewModels;

namespace WorkStatus.Views
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainWindowViewModel();

            this.AddHandler(PointerPressedEvent, MouseDownHandler, handledEventsToo: true);
            this.AddHandler(PointerReleasedEvent, MouseUpHandler, handledEventsToo: true);

#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void MouseUpHandler(object? sender, PointerReleasedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Mouse released.");
        }

        private void MouseDownHandler(object? sender, PointerPressedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Mouse pressed.");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
