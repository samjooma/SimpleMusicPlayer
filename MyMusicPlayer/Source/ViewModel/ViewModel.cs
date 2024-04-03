using Microsoft.VisualBasic;
using MyMusicPlayer.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using MS.WindowsAPICodePack.Internal;
using System.Diagnostics;

namespace MyMusicPlayer.ViewModel
{
    public class DirectoryHierarchy : INotifyPropertyChanged
    {
        private Model.DirectoryHierarchy DirectoryData { get; set; }
        private Dictionary<DirectoryInfo, Directory> DirectoryMap { get; set; }
        private Directory? _selectedDirectory;
        public Directory? SelectedDirectory
        {
            get => _selectedDirectory;
            private set
            {
                if (value != _selectedDirectory)
                {
                    _selectedDirectory = value;
                    NotifyPropertyChanged(nameof(SelectedDirectory));
                }
            }
        }

        public Directory RootDirectory { get => DirectoryMap[DirectoryData.RootDirectory]; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public DirectoryHierarchy()
        {
            DirectoryMap = new Dictionary<DirectoryInfo, Directory>(new DirectoryComparer());
            DirectoryData = new Model.DirectoryHierarchy();
        }

        public void SetRootDirectory(string RootDirectoryPath)
        {
            DirectoryMap.Clear();
            DirectoryData = new Model.DirectoryHierarchy();
            DirectoryData.AfterDirectoryOpened += DirectoryData_AfterDirectoryOpened;
            DirectoryData.AfterDirectoryClosed += DirectoryData_AfterDirectoryClosed;
            DirectoryData.OpenDirectory(RootDirectoryPath);
        }

        public Directory[] GetAllDirectories()
        {
            return DirectoryMap.Values.ToArray();
        }

        private void TryAddNewDirectory(DirectoryInfo Key)
        {
            var Value = new Directory(Key);
            if (DirectoryMap.TryAdd(Key, Value))
            {
                Value.PropertyChanged += Directory_PropertyChanged;
            }
        }

        private void Directory_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not Directory ChangedDirectory) return;
            switch (e.PropertyName)
            {
                case nameof(ChangedDirectory.IsSelected):
                    if (ChangedDirectory.IsSelected)
                    {
                        // Deselect currently selected directory.
                        if (SelectedDirectory != null)
                        {
                            SelectedDirectory.IsSelected = false;
                        }
                        // Set reference to new selected object.
                        SelectedDirectory = ChangedDirectory;
                    }
                    else
                    {
                        SelectedDirectory = null;
                    }
                    break;
                case nameof(ChangedDirectory.IsExpanded):
                    foreach (var Child in ChangedDirectory.Children)
                    {
                        DirectoryInfo Key = DirectoryMap.First(x => x.Value == Child).Key;
                        DirectoryData.OpenDirectory(Key);
                    }
                    break;
                default:
                    break;
            }
        }

        private void DirectoryData_AfterDirectoryOpened(object? sender, DirectoryOpenedEventArgs e)
        {
            TryAddNewDirectory(e.OpenedDirectory);
            DirectoryMap[e.OpenedDirectory].Children.Clear();
            foreach (DirectoryInfo SubDirectory in DirectoryData.GetSubDirectories(e.OpenedDirectory))
            {
                TryAddNewDirectory(SubDirectory);
                DirectoryMap[e.OpenedDirectory].Children.Add(DirectoryMap[SubDirectory]);
            }

            NotifyPropertyChanged(nameof(DirectoryMap));
            NotifyPropertyChanged(nameof(RootDirectory));
        }

        private void DirectoryData_AfterDirectoryClosed(object? sender, DirectoryClosedEventArgs e)
        {
        }

        protected virtual void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }

    public class Directory : INotifyPropertyChanged
    {
        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    NotifyPropertyChanged(nameof(IsExpanded));
                }
            }
        }
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    NotifyPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public string Name { get; private set; }
        public string FullName { get; private set; }

        public List<FileInfo> Files { get; private set; }
        public ObservableCollection<Directory> Children { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public Directory(string Name, string FullName, List<FileInfo> Files)
        {
            _isExpanded = false;
            _isSelected = false;
            Children = new ObservableCollection<Directory>();
            this.Name = Name;
            this.FullName = FullName;
            this.Files = Files;
        }

        public Directory(DirectoryInfo Info) : this(
            Info.Name, Info.FullName,
            Info.GetFiles("*", SearchOption.TopDirectoryOnly).ToList())
        {
        }

        protected virtual void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
