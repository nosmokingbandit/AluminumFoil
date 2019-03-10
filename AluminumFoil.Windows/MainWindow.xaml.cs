using System.Windows;
using System.Windows.Controls;
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
