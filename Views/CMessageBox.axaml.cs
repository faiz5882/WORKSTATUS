using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;


namespace WorkStatus.Views
{
    public partial class CMessageBox : Window
    {
        static CMessageBox cMessageBox;
        public CMessageBox()
        {
            this.HasSystemDecorations = false;
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void quitapp_Click(object sender, RoutedEventArgs e)
        {

            this.Close();


        }
        private void cancelapp_Click(object sender, RoutedEventArgs e)
        {

            this.Close();


        }
        private void minimizeapp_Click(object sender, RoutedEventArgs e)
        {

            this.Close();


        }
    }
}
