using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;

namespace AluminumFoil.Posix.Dialogs
{
    public class Success : Views.AutoHeightWindow
    {
        public Success(string Title, string Message)
        {
            MinWidth = 300;
            Width = 300;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            CanResize = false;
            DataContext = new ViewModels.Dialog(Title, Message);
            AvaloniaXamlLoader.Load(this);

            MainPanel = this.Find<StackPanel>("MainPanel");
        }

        private void HandleClose(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
