using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;

namespace AluminumFoil.Posix.Dialogs
{
    public class Error : Views.AutoHeightWindow
    {
        public Error(string Title, string Message, string errmessage = "")
        {
            MinWidth = 300;
            Width = 300;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            CanResize = false;
            DataContext = new ViewModels.Dialog(Title, Message);
            AvaloniaXamlLoader.Load(this);

            MainPanel = this.Find<StackPanel>("MainPanel");
        }

        void HandleClose(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Close");
            Close();
        }
    }
}
