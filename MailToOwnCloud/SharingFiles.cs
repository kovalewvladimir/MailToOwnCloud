﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WebDAVClient;
using WebDAVClient.Helpers;

namespace MailToOwnCloud
{
    class SharingFiles
    {
        private ObservableCollection<UploadFile> _uploadFiles;
        //public ObservableCollection<UploadFile> UploadFiles { get => _uploadFiles; set => _uploadFiles = value; }

        private string _rootPathClient;
        private DataGrid _dataGrid;

        public SharingFiles(DataGrid dataGrid)
        {
            _uploadFiles = new ObservableCollection<UploadFile>();
            _dataGrid    = dataGrid;
        }

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

        public async Task<string> Upload(string server, string login, string password)
        {
            IClient c = new Client(new System.Net.NetworkCredential { UserName = login, Password = password }, TimeSpan.FromHours(1))
            {
                Server = $"http://{server}/",
                BasePath = $"/remote.php/dav/files/{login}/"
            };

            try
            {
                string rootPathServer = await this.CreateRootPathServer(c);

            
                var isCreateDirectory = await this.CreateDirectory(c, rootPathServer);
                var isUploadFiles = await this.UploadFiles(c, rootPathServer);

                if (isCreateDirectory && isUploadFiles)
                {
                    return await this.GetPublicLink(server, login, password, rootPathServer);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
           
        }

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

        public async Task<ObservableCollection<UploadFile>> GetFilesAsync(string[] pathNames)
        {
            return await Task.Factory.StartNew(()=> this.GetFiles(pathNames));
        }
    }
}
