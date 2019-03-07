using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AluminumFoil.Mac.Views
{
    public class MainWindow : AutoHeightWindow
    {
        public MainWindow()
        {
            this.Width = 400;
            this.MinWidth = 400;
            this.CanResize = false;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            this.DataContext = new ViewModels.MainWindow();
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            MainPanel = this.Find<StackPanel>("MainPanel");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
