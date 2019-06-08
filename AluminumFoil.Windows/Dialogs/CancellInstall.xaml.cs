using System.Windows;

namespace AluminumFoil.Dialogs
{
    public partial class CancelInstall : Window
    {
        public CancelInstall()
        {
            InitializeComponent();
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void CloseAndCancel(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
