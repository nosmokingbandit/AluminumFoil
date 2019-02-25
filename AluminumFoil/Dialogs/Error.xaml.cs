using System.Windows;

namespace AluminumFoil.Dialogs
{
    public partial class Error : Window
    {
        public Error(string Title, string Message)
        {
            Owner = Application.Current.MainWindow;
            DataContext = new ViewModels.Dialog(Title, Message);
            InitializeComponent();
        }

        private void HandleClose(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
