using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using System.Threading.Tasks;

namespace WorkStatus.Views
{
    public class MyMessageBox : Window
    {
        public enum MessageBoxButtons
        {
            Ok,
            OkCancel,
            YesNo,
            YesNoCancel,
            Continue
        }

        public enum MessageBoxResult
        {
            Ok,
            Cancel,
            Yes,
            No,
            Continue
        }


        public MyMessageBox()
        {
            this.HasSystemDecorations = false;
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        public static Task<MessageBoxResult> Show(Window parent, string text, string title, MessageBoxButtons buttons)
        {
            var msgbox = new MyMessageBox()
            {
                Title = title
            };
            msgbox.FindControl<TextBlock>("Text").Text = text;
            var buttonPanel = msgbox.FindControl<StackPanel>("Buttons");

            var res = MessageBoxResult.Ok;

            void AddButton(string caption, MessageBoxResult r, bool def = false, HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center)
            {
                var btn = new Button { Content = caption };
                btn.HorizontalAlignment = horizontalAlignment;

                var colorBrushWhite = (Avalonia.Media.SolidColorBrush)(new Avalonia.Media.BrushConverter().ConvertFromString("#FFFFFF"));

                var colorBrushBlue = (Avalonia.Media.SolidColorBrush)(new Avalonia.Media.BrushConverter().ConvertFromString("#1665D8"));


                btn.Background = colorBrushBlue;
                btn.Foreground = colorBrushWhite;
                btn.BorderBrush = colorBrushBlue;
                btn.Width = 70;
                btn.Click += (_, __) => {
                    res = r;
                    msgbox.Close();
                };
                buttonPanel.Children.Add(btn);
                if (def)
                    res = r;
            }

            if (buttons == MessageBoxButtons.Ok || buttons == MessageBoxButtons.OkCancel)
                AddButton("Ok", MessageBoxResult.Ok, true);

            if (buttons == MessageBoxButtons.YesNo || buttons == MessageBoxButtons.YesNoCancel)
            {
                AddButton("Yes", MessageBoxResult.Yes);
                AddButton("No", MessageBoxResult.No, true);
            }

            if (buttons == MessageBoxButtons.OkCancel || buttons == MessageBoxButtons.YesNoCancel)
                AddButton("Cancel", MessageBoxResult.Cancel, true);

            if (buttons == MessageBoxButtons.Continue)
                AddButton("Continue", MessageBoxResult.Continue, true);


            var tcs = new TaskCompletionSource<MessageBoxResult>();
            msgbox.WindowStartupLocation = WindowStartupLocation.CenterOwner;


            msgbox.Closed += delegate { tcs.TrySetResult(res); };
            if (parent != null)
                msgbox.ShowDialog(parent);
            else msgbox.Show();
            return tcs.Task;
        }
    }
}
