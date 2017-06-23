using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Reflection;

namespace NTLinksMaker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Assembly myAssembly = Assembly.GetEntryAssembly();
        public static string ExecutablePath = myAssembly.Location;
        public static bool PreferSymlink = Utils.DeveloperModeEnabled();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var args = e.Args;
            if (args.Length > 4 && args[0] == "/admin")
            {
                try
                {
                    Utils.CreateLinkByAdmin(args);
                }
                catch (Exception ex)
                {
                    App.ShowError(ex.Message);
                }
                this.Shutdown();
            }
        }

        public static MessageBoxResult ShowError(string message)
        {
            return MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
