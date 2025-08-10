using QI_CMS_v2.ViewModels;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace QI_CMS_v2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [DllImport("shcore.dll")]
        private static extern int SetProcessDpiAwareness(int awareness);

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var mainWindow = new MainWindow()
            {
                DataContext = new MainWindowViewModel()
            };
            mainWindow.Show();
        }
    }

}
