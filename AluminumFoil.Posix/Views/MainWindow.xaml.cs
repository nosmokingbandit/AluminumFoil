using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AluminumFoil.Posix.Views
{
    public class MainWindow : AutoHeightWindow
    {
        public MainWindow()
        {
            Width = 400;
            MinWidth = 400;
            CanResize = false;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            DataContext = new ViewModels.MainWindow();
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            MainPanel = this.Find<StackPanel>("MainPanel");
        }

        private void ChangeInstallationTarget(object sender, object e)
        {
            DropDown dd = (DropDown)sender;
            DropDownItem ddi = (DropDownItem)dd.SelectedItem;

            var dc = (ViewModels.MainWindow)DataContext;
            dc.InstallationTarget = ddi.Content.ToString();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
