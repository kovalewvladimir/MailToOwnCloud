using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using WebDAVClient;
using WebDAVClient.Helpers;

namespace MailToOwnCloud
{
    /// <summary>
    /// Вспомогательный класс для работы с файлами и отправки их на сервер OwnCloud
    /// </summary>
    class SharingFiles
    {

        #region Локальные переменные класса

        private ObservableCollection<UploadFile> _uploadFiles;

        private string _rootPathClient;
        private DataGrid _dataGrid;

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="dataGrid">view для отображения файлов</param>
        public SharingFiles(DataGrid dataGrid)
        {
            _uploadFiles = new ObservableCollection<UploadFile>();
            _dataGrid    = dataGrid;
        }

        #endregion

        #region private методы

        private void SelectedItemDataGrid(UploadFile item)
        {
            _dataGrid.ScrollIntoView(item);
            //_dataGrid.SelectedItem = item;
        }

        private async Task<string> CreateRootPathServer(IClient c)
        {
            var userName = Environment.UserName;
            var rootPathServer = userName + "\\" + Guid.NewGuid().ToString() + "\\";
            try
            {
                await c.CreateDir("/", userName);
            }
            catch (WebDAVException) { }
            await c.CreateDir("/", rootPathServer);

            return rootPathServer;
        }

        private async Task<bool> CreateDirectory(IClient c, string rootPathServer)
        {
            // TODO: убрать isFolderCreated

            var isFolderCreated = true;

            var uploadDirectory = _uploadFiles.Where(item => item.TypePath == TypePath.Directory);
            foreach (var directory in uploadDirectory)
            {
                SelectedItemDataGrid(directory);
                directory.Status = "Отправляю..";

                if (!isFolderCreated) { return false; }

                var dir = directory.Path.Replace(_rootPathClient, "");
                try
                {
                    isFolderCreated = await c.CreateDir(rootPathServer, dir);
                    directory.Status = isFolderCreated.ToString();
                }
                catch (WebDAVException)
                {
                    directory.Status = "Error";
                }
            }
            return isFolderCreated;
        }

        private async Task<bool> UploadFiles(IClient c, string rootPathServer)
        {
            var isFileCreated = true;

            var uploadfile = _uploadFiles.Where(item => item.TypePath == TypePath.File);
            foreach (var file in uploadfile)
            {
                SelectedItemDataGrid(file);
                file.Status = "Отправляю..";

                if (!isFileCreated) { return false; }

                using (var fileStream = File.OpenRead(file.Path))
                {
                    var remoteFilePath = rootPathServer + Path.GetDirectoryName(file.Path.Replace(_rootPathClient, ""));
                    var name = Path.GetFileName(file.Path);

                    isFileCreated = await c.Upload(remoteFilePath, fileStream, name);
                    file.Status = isFileCreated.ToString();
                }
            }
            return isFileCreated;
        }

        private async Task<string> GetPublicLink(string server, string login, string password, string path)
        {
            using (HttpClient client = new HttpClient())
            {
                var byteArray = Encoding.ASCII.GetBytes($"{login}:{password}");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                var values = new Dictionary<string, string>
                {
                    { "path", path },
                    { "shareType", "3"},
                    { "permissions", "1"},
                };

                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync($"http://{server}/ocs/v1.php/apps/files_sharing/api/v1/shares?format=json", content);
                var responseString = await response.Content.ReadAsStringAsync();
                responseString = responseString.Replace("\\", "");

                Regex regex = new Regex(@"(http://[\w|/|.|-]*)");  
                Match match = regex.Match(responseString);
              
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }

                return null;
            }
        }

        #endregion

        #region public методы

        /// <summary>
        /// Метод отправляет файлы на сервер (webdav)
        /// </summary>
        /// <param name="server">Сервер OwnCloud</param>
        /// <param name="login">Логин</param>
        /// <param name="password">Пароль</param>
        /// <returns>Публичная ссылка</returns>
        public async Task<string> Upload(string server, string login, string password)
        {
            IClient c = new Client(new System.Net.NetworkCredential { UserName = login, Password = password }, TimeSpan.FromHours(1))
            {
                Server = $"http://{server}/",
                BasePath = $"/remote.php/dav/files/{login}/"
            };

            string rootPathServer = await this.CreateRootPathServer(c);

            bool isCreateDirectory = await this.CreateDirectory(c, rootPathServer);
            bool isUploadFiles     = await this.UploadFiles(c, rootPathServer);

            if (isCreateDirectory && isUploadFiles)
            {
                return await this.GetPublicLink(server, login, password, rootPathServer);
            }

            return null;
        }

        /// <summary>
        /// Получить cписок файлов/папок
        /// </summary>
        /// <param name="pathNames">Массив с путями до файлов/папок</param>
        /// <returns>Коллекция с файлами и папками</returns>
        public ObservableCollection<UploadFile> GetFiles(string[] pathNames)
        {
            if (_rootPathClient == null)
            {
                string dir = Path.GetDirectoryName(pathNames[0]);
                _rootPathClient = (dir[dir.Length - 1] != '\\') ? dir + "\\" : dir;
            }
            foreach (string path in pathNames)
            {
                if (File.Exists(path))
                {
                    _uploadFiles.Add(new UploadFile(path, TypePath.File));
                }
                else
                {
                    if (Directory.Exists(path))
                    {
                        _uploadFiles.Add(new UploadFile(path, TypePath.Directory));
                        this.GetFiles(Directory.GetDirectories(path));
                        this.GetFiles(Directory.GetFiles(path));
                    }
                }
            }
            return _uploadFiles;
        }

        /// <summary>
        /// Получить cписок файлов/папок асинхронно 
        /// </summary>
        /// <param name="pathNames">Массив с путями до файлов/папок</param>
        /// <returns>Коллекция с файлами и папками<</returns>
        public async Task<ObservableCollection<UploadFile>> GetFilesAsync(string[] pathNames)
        {
            return await Task.Factory.StartNew(()=> this.GetFiles(pathNames));
        }

        #endregion
    }
}
