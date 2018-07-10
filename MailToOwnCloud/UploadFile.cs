using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MailToOwnCloud
{
    enum TypePath
    {
        File,
        Directory
    }

    class UploadFile : INotifyPropertyChanged
    {
        private string _path;
        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                NotifyPropertyChanged();
            }
        }

        private TypePath _typePath;
        public TypePath TypePath
        {
            get { return _typePath; }
            set
            {
                _typePath = value;
                NotifyPropertyChanged();
            }
        }
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

        private string _status;
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                NotifyPropertyChanged();
            }
        }

        public UploadFile(string path, TypePath typePath, string status)
        {
            this.Path     = path;
            this.TypePath = typePath;
            this.Status   = status;
        }

        public UploadFile(string path, TypePath typePath) : this(path, typePath, "Ожидает") { }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
