using System.Windows;
using System.Linq;
namespace AluminumFoil.Windows
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DataContext = new ViewModels.MainWindow();
            InitializeComponent();

            this.Closing += HandleClosing;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            this.MinHeight = this.DesiredSize.Height;
            this.MaxHeight = this.DesiredSize.Height - GroupBoxNSPs.DesiredSize.Height + GroupBoxNSPs.MaxHeight;
        }

        private void VerifyDragNSPs(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Any(f => !f.EndsWith(".nsp")))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        private void OpenNSPDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            ViewModels.MainWindow dc = (ViewModels.MainWindow)this.DataContext;
            dc.OpenNSPs(files);
        }

        private void HandleClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var vm = this.DataContext as ViewModels.MainWindow;
            if (!vm.AllowActions)
            {
                e.Cancel = true;
                return;
            }
        }
    }
}
