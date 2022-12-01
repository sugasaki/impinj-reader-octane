using ImpinjReader.Views;
using System.Windows;

namespace ImpinjReader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// アプリケーション起動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var window = new MainWindow();
            window.Show();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
        }
    }
}
