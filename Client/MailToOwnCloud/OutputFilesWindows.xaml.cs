using System;
using System.Windows;
using System.Configuration;
using System.IO;
using System.Linq;

namespace MailToOwnCloud
{
    /// <summary>
    /// Логика взаимодействия для OutputFilesWindows.xaml
    /// Главное окно приложения
    /// </summary>
    public partial class OutputFilesWindows : Window
    {
        #region Локальные переменные класса

        private SharingFiles _sharingFiles;
        private string[] _args;

        private string _server;
        private string _login;
        private string _password;

        private string _thunderbirdExe;
        private string _thunderbirdArgs;

        private bool   _thunderbirdIsDays;
        private int    _thunderbirdDays;
        private string _thunderbirdDaysText;
        private string _thunderbirdFilesText1;
        private string _thunderbirdFilesText2;
        private string _thunderbirdFilesText3;

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор класса OutputFilesWindows
        /// </summary>
        /// <param name="args">Аргументы командной строки (путь до файла/папки)</param>
        public OutputFilesWindows(string[] args)
        {
            // Инициализация компонентов
            InitializeComponent();

            // Инициализация локальных переменных
            // Аргументы командной строки (путь до файлов/папок)
            _args = args;

            // Значения хранятся в файле
            // app.config (до компиляции) -> MailToOwnCloud.exe.config (после компиляции)
            _server = ConfigurationManager.AppSettings["server"];
            _login = ConfigurationManager.AppSettings["login"];
            try
            {
                _password = EncryptionHelper.Decrypt(ConfigurationManager.AppSettings["password"]);
            }
            catch (System.Security.Cryptography.CryptographicException ex)
            {
                MessageBoxShow.Error(ex.Message, "", "Не могу расшифровать пароль в конфигурационном файле");
                App.Current.Shutdown(1);
            }

            _thunderbirdExe  = ConfigurationManager.AppSettings["thunderbird_exe"];
            _thunderbirdArgs = ConfigurationManager.AppSettings["thunderbird_args"];

            try
            {
                _thunderbirdIsDays = bool.Parse(ConfigurationManager.AppSettings["thunderbird_is_days"]);
                _thunderbirdDays = int.Parse(ConfigurationManager.AppSettings["thunderbird_days"]);
                _thunderbirdDaysText = ConfigurationManager.AppSettings["thunderbird_days_text"];
                _thunderbirdFilesText1 = ConfigurationManager.AppSettings["thunderbird_files_text1"];
                _thunderbirdFilesText2 = ConfigurationManager.AppSettings["thunderbird_files_text2"];
                _thunderbirdFilesText3 = ConfigurationManager.AppSettings["thunderbird_files_text3"];
            }
            catch (Exception ex)
            {
                MessageBoxShow.Error(ex.Message, "", "Ошибка в конфигурационном файле");
                App.Current.Shutdown(1);
            }

            // Проверка проинициализированы ли локальные переменные
            if (
                _server              == null ||
                _login               == null ||
                _password            == null ||
                _thunderbirdExe      == null ||
                _thunderbirdArgs     == null ||
                _thunderbirdDaysText == null ||
                _thunderbirdFilesText1 == null ||
                _thunderbirdFilesText2 == null ||
                _thunderbirdFilesText3 == null
                )
            {
                MessageBoxShow.Error("Ошибка в конфигурационном файле");
                App.Current.Shutdown(1);
            }

            // Инициализация MessageBox`ов
            MessageBoxShow.MessageErrorTitle = ConfigurationManager.AppSettings["message_error_title"];
            MessageBoxShow.MessageErrorText  = ConfigurationManager.AppSettings["message_error"];

            // Проверка есть ли файлы для отправки на сервер
            if (_args.Length == 0)
            {
                MessageBoxShow.Error("Нет файлов для отправки");
                App.Current.Shutdown(1);
            }

            if (! AuthenticationHelper.Authentication("access.csv"))
            {
                MessageBoxShow.Error("Нет прав. Обратитесь к системному администратору для получения соответствующих прав доступа.");
                App.Current.Shutdown(1);
            }
        }

        #endregion

        #region События

        /// <summary>
        /// Событие загрузки окна
        /// </summary>
        /// <param name="sender">Объект окна</param>
        /// <param name="e">Информация о событии</param>
        private async void outputFilesWindows_Loaded(object sender, RoutedEventArgs e)
        {
            _sharingFiles = new SharingFiles(dg_files);
            try
            {
                dg_files.DataContext = await _sharingFiles.GetFilesAsync(_args);
                dg_files.Visibility = Visibility.Visible;

                btn_upload.IsEnabled = true;
                img_btn_upload.Visibility = Visibility.Collapsed;
                txtbl_btn_upload.Text = "Отправить по почте";

            }
            catch (Exception ex)
            {
                MessageBoxShow.Error($"\n{ex.Message}");
                App.Current.Shutdown(1);
            }
        }

        /// <summary>
        /// Событие click по кнопке btn_upload
        /// </summary>
        /// <param name="sender">Объект кнопки</param>
        /// <param name="e">Информация о событии</param>
        private async void btn_upload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btn_upload.IsEnabled = false;
                txtbl_btn_upload.Text = "Отправляю...";
                img_btn_upload.Visibility = Visibility.Visible;

                string publicLink = await _sharingFiles.Upload(_server, _login, _password);

                if (publicLink == null)
                {
                    throw new Exception("Не получилось создать ссылку");
                }

                string date = (DateTime.Now + TimeSpan.FromDays(_thunderbirdDays)).ToString("dd.MM.yyyy");

                var listfiles = _sharingFiles.GetUploadFiles
                     .Where(file => file.TypePath == TypePath.File)
                     .Select(file => file.Path.Replace(_args[0] + "\\", ""))
                     .Aggregate((cur, next) => cur + "<br> " + next); ;

                string body = (_thunderbirdIsDays) ? $"{publicLink}<br>{String.Format(_thunderbirdDaysText, date)}<br><br>{String.Format(_thunderbirdFilesText1, _thunderbirdFilesText2)}<br>{String.Format(_thunderbirdFilesText3, listfiles)}" : publicLink;

                System.Diagnostics.Process.Start(_thunderbirdExe, String.Format(_thunderbirdArgs, body));
            }
            catch (Exception ex)
            {
                MessageBoxShow.Error($"\n{ex.Message}");
            }

            this.Close();
        }
        
        #endregion
    }
}
