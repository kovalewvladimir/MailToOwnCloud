using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WebDAVClient;

namespace MailToOwnCloud
{
    /// <summary>
    /// Логика взаимодействия для OutputFilesWindows.xaml
    /// </summary>
    public partial class OutputFilesWindows : Window
    {
        private SharingFiles _sharingFiles;
        private string[] _args;

        private string _server;
        private string _login;
        private string _password;

        public OutputFilesWindows(string[] args)
        {
            InitializeComponent();

            _args = args;

            try
            {
                _server   = Properties.Settings.Default.server;
                _login    = Properties.Settings.Default.login;
                _password = Properties.Settings.Default.password;
            }
            catch (System.Configuration.SettingsPropertyNotFoundException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }           
        }

        private async void outputFilesWindows_Loaded(object sender, RoutedEventArgs e)
        {
            _sharingFiles = new SharingFiles(dg_files);
            dg_files.DataContext = await _sharingFiles.GetFilesAsync(_args);
        }

        private async void btn_upload_Click(object sender, RoutedEventArgs e)
        { 
            try
            {
                btn_upload.IsEnabled = false;
                btn_upload.Content = "Отправляю..";

                string publicLink = await _sharingFiles.Upload(_server, _login, _password);

                if (publicLink == null)
                {
                    MessageBox.Show("Error! Обратитесь к системному администратору", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    //MessageBox.Show(publicLink);
                    System.Diagnostics.Process.Start(@"C:\Program Files (x86)\Mozilla Thunderbird\thunderbird.exe", $"-compose body={publicLink}");
                    btn_upload.IsEnabled = true;
                }
                
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
