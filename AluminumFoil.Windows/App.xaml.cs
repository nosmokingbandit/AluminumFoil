using System.Windows;
using System;

namespace AluminumFoil.Windows
{
    public partial class App : Application
    {
        public static GoldLeaf GoldLeaf = new GoldLeaf(); // Buffalo buffalo Buffalo buffalo buffalo buffalo Buffalo buffalo
        public static TinFoil.TinFoil TinFoil = new TinFoil.TinFoil();

        public void StartApp(object sender, StartupEventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i] == "--log")
                {
                    Logging.SetupLogging();
                }
            }

            MainWindow mw = new MainWindow();
            mw.Show();

            mw.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Logging.StopLogging();
        }
    }
}
