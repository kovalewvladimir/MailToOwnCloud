using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MailToOwnCloud
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length > 1 && e.Args[0] == "--new-password")
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings.Remove("password");
                config.AppSettings.Settings.Add("password", EncryptionHelper.Encrypt(e.Args[1]));
                config.Save();
                App.Current.Shutdown(0);
            }
            else
            {
                OutputFilesWindows outputFilesWindows = new OutputFilesWindows(e.Args);
                outputFilesWindows.Show();
            }
        }
    }
}
