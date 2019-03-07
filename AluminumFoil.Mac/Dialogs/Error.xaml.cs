using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;

namespace AluminumFoil.Mac.Dialogs
{
    public class Error : Views.AutoHeightWindow
    {
        public Error(string Title, string Message)
        {
            this.MinWidth = 300;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.CanResize = false;
            this.DataContext = new Mac.ViewModels.Dialog(Title, Message);
            AvaloniaXamlLoader.Load(this);

            MainPanel = this.Find<StackPanel>("MainPanel");
        }

        private void HandleClose(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
