using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MailToOwnCloud
{
    /// <summary>
    /// Тип пути
    /// </summary>
    enum TypePath
    {
        File,
        Directory
    }

    /// <summary>
    /// Класс данных
    /// </summary>
    class UploadFile : INotifyPropertyChanged
    {
        #region Локальные переменные класса

        private string _path;
        private TypePath _typePath;
        private string _status;

        #endregion

        #region Свойства

        /// <summary>
        /// Путь до файла/папки
        /// </summary>
        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Тип файл/папка
        /// </summary>
        public TypePath TypePath
        {
            get { return _typePath; }
            set
            {
                _typePath = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Тип файл/папка
        /// </summary>
        public string TypePathToString
        {
            get
            {
                switch (TypePath)
                {
                    case TypePath.File:
                        return "Файл";
                    case TypePath.Directory:
                        return "Папка";
                }
                return "";
            }
        }

        /// <summary>
        /// Статус
        /// </summary>
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="path">Путь</param>
        /// <param name="typePath">Тип</param>
        /// <param name="status">Статус</param>
        public UploadFile(string path, TypePath typePath, string status)
        {
            this.Path     = path;
            this.TypePath = typePath;
            this.Status   = status;
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="path">Путь</param>
        /// <param name="typePath">Тип</param>
        public UploadFile(string path, TypePath typePath) : this(path, typePath, "Ожидает") { }

        #endregion

        #region Событие для динамического обновления view

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
