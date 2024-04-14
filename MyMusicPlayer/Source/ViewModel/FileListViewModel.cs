using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMusicPlayer.ViewModel
{
    public class FileList : INotifyPropertyChanged
    {
        private ObservableCollection<FileInfo> _files;
        private HashSet<int> _selectedIndices;
        private int? _activeIndex;

        public IEnumerable<FileInfo> Files { get { foreach (var File in _files) yield return File; } }
        public IEnumerable<FileInfo> SelectedFiles { get { foreach (int Index in _selectedIndices) yield return _files[Index]; } }
        public FileInfo? ActiveFile { get => _activeIndex != null ? _files[_activeIndex.Value] : null; }

        public bool AllowMultipleSelection { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public FileList(bool AllowMultipleSelection)
        {
            _files = new ObservableCollection<FileInfo>();
            _files.CollectionChanged += Files_CollectionChanged;
            _selectedIndices = new HashSet<int>();
            this.AllowMultipleSelection = AllowMultipleSelection;
        }

        private void Files_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _selectedIndices.Clear();
            _activeIndex = null;
            NotifyPropertyChanged(nameof(Files));
            NotifyPropertyChanged(nameof(SelectedFiles));
            NotifyPropertyChanged(nameof(ActiveFile));
        }

        public void SetSelection(int Index)
        {
            _selectedIndices = [Index];
            _activeIndex = Index;
            NotifyPropertyChanged(nameof(SelectedFiles));
            NotifyPropertyChanged(nameof(ActiveFile));
        }

        public void ExpandSelection(int Index)
        {
            if (AllowMultipleSelection)
            {
                _selectedIndices.Add(Index);
                _activeIndex = Index;
                NotifyPropertyChanged(nameof(SelectedFiles));
                NotifyPropertyChanged(nameof(ActiveFile));
            }
            else
            {
                SetSelection(Index);
            }
        }

        public void AddFile(FileInfo File)
        {
            _files.Add(File);
        }

        public void AddFiles(IEnumerable<FileInfo> Files)
        {
            foreach (var File in Files)
            {
                AddFile(File);
            }
        }

        public void SetFiles(IEnumerable<FileInfo> Files)
        {
            _files.Clear();
            AddFiles(Files);
        }

        public void ClearFiles()
        {
            _files.Clear();
        }

        protected virtual void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
