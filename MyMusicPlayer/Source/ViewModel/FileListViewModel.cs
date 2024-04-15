using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MyMusicPlayer.ViewModel
{
    public class FileSelectionList : INotifyPropertyChanged
    {
        protected List<FileInfo> _files;

        public IEnumerable<FileInfo> Files { get { foreach (var File in _files) yield return File; } }

        public bool AllowMultipleSelection { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public FileSelectionList(bool AllowMultipleSelection)
        {
            _files = new List<FileInfo>();
            this.AllowMultipleSelection = AllowMultipleSelection;
        }

        public void AddFile(FileInfo File)
        {
            _files.Add(File);
            NotifyPropertyChanged(nameof(Files));
        }

        public void AddFiles(IEnumerable<FileInfo> Files)
        {
            _files.AddRange(Files);
            NotifyPropertyChanged(nameof(Files));
        }

        public void SetFiles(IEnumerable<FileInfo> Files)
        {
            _files = new List<FileInfo>(Files);
            NotifyPropertyChanged(nameof(Files));
        }

        public void RemoveFiles(IEnumerable<FileInfo> FilesToRemove)
        {
            _files.RemoveAll(x => FilesToRemove.Contains(x));
            NotifyPropertyChanged(nameof(Files));
        }

        public void ClearFiles()
        {
            _files.Clear();
            NotifyPropertyChanged(nameof(Files));
        }

        protected virtual void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
