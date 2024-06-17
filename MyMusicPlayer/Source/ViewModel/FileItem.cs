using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMusicPlayer.ViewModel
{
    public class FileItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public System.IO.FileInfo FileInfo { get; private set; }
        public string Name { get { return System.IO.Path.GetFileNameWithoutExtension(FileInfo.Name); } }
        public string Extension { get { return FileInfo.Extension; } }

        public FileItem(System.IO.FileInfo FileInfo)
        {
            this.FileInfo = FileInfo;
        }

        protected virtual void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }

    public class SongQueueItem : FileItem
    {
        private bool _isSongActive;
        public bool IsSongActive {
            get { return _isSongActive; }
            set
            {
                if (value != _isSongActive)
                {
                    _isSongActive = value;
                    NotifyPropertyChanged(nameof(IsSongActive));
                }
            }
        }

        public SongQueueItem(System.IO.FileInfo FileInfo, bool IsSongActive) : base(FileInfo)
        {
            _isSongActive = IsSongActive;
        }
    }
}
