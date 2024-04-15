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
    public class FileSelectionList : INotifyPropertyChanged
    {
        protected ObservableCollection<FileInfo> _files;
        protected HashSet<int> _selectedIndices;

        public IEnumerable<FileInfo> Files { get { foreach (var File in _files) yield return File; } }
        public IEnumerable<FileInfo> SelectedFiles { get { foreach (int Index in _selectedIndices) yield return _files[Index]; } }

        public bool AllowMultipleSelection { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public FileSelectionList(bool AllowMultipleSelection)
        {
            _files = new ObservableCollection<FileInfo>();
            _files.CollectionChanged += Files_CollectionChanged;
            _selectedIndices = new HashSet<int>();
            this.AllowMultipleSelection = AllowMultipleSelection;
        }

        protected virtual void Files_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _selectedIndices.Clear();
            NotifyPropertyChanged(nameof(Files));
            NotifyPropertyChanged(nameof(SelectedFiles));
        }

        protected virtual void SetSelection(int Index)
        {
            _selectedIndices = [Index];
            NotifyPropertyChanged(nameof(SelectedFiles));
        }

        protected virtual void ExpandSelection(int Index)
        {
            if (AllowMultipleSelection)
            {
                _selectedIndices.Add(Index);
                NotifyPropertyChanged(nameof(SelectedFiles));
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

    public class FileActivationList : FileSelectionList
    {
        private int _activeIndex;
        public int ActiveIndex
        {
            get => _activeIndex;
            set
            {
                if (value != _activeIndex)
                {
                    _activeIndex = value;
                    NotifyPropertyChanged(nameof(ActiveIndex));
                    NotifyPropertyChanged(nameof(ActiveFile));
                }
            }
        }

        public FileInfo? ActiveFile { get => ActiveIndex > -1 ? _files[ActiveIndex] : null; }

        public FileActivationList(bool AllowMultipleSelection) : base(AllowMultipleSelection)
        {
            ActiveIndex = -1;
        }

        protected override void Files_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.Files_CollectionChanged(sender, e);
            ActiveIndex = -1;
        }

        protected override void SetSelection(int Index)
        {
            base.SetSelection(Index);
            ActiveIndex = Index;
        }

        protected override void ExpandSelection(int Index)
        {
            base.SetSelection(Index);
            ActiveIndex = Index;
        }
    }
}
