using Microsoft.VisualBasic;
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
using System.Windows.Input;

namespace MyMusicPlayer.ViewModel
{
    public class DirectoryTree : INotifyPropertyChanged
    {
        private Model.DirectoryHierarchy DirectoryData { get; set; }
        private Dictionary<DirectoryInfo, FileTreeDirectory> DirectoryMap { get; set; }
        private FileTreeItem? _selectedItem;
        public FileTreeItem? SelectedItem
        {
            get => _selectedItem;
            private set
            {
                if (value != _selectedItem)
                {
                    _selectedItem = value;
                    NotifyPropertyChanged(nameof(SelectedItem));
                }
            }
        }

        public FileTreeDirectory RootDirectory { get => DirectoryMap[DirectoryData.RootDirectory]; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public DirectoryTree()
        {
            DirectoryMap = new Dictionary<DirectoryInfo, FileTreeDirectory>(new Model.DirectoryComparer());
            DirectoryData = new Model.DirectoryHierarchy();
        }

        public void SetRootDirectory(string RootDirectoryPath)
        {
            DirectoryMap.Clear();
            DirectoryData = new Model.DirectoryHierarchy();

            var RootDirectory = new DirectoryInfo(RootDirectoryPath);
            TryAddToDirectoryDictionary(RootDirectory);

            DirectoryData.AfterDirectoryOpened += DirectoryData_AfterDirectoryOpened;
            DirectoryData.AfterDirectoryClosed += DirectoryData_AfterDirectoryClosed;
            DirectoryData.OpenDirectory(RootDirectory);
        }

        //TODO: This function is bad.
        public FileTreeDirectory[] GetAllDirectories()
        {
            return DirectoryMap.Values.ToArray();
        }

        private void TryAddToDirectoryDictionary(DirectoryInfo Key)
        {
            var Value = new FileTreeDirectory(Key.Name);
            if (DirectoryMap.TryAdd(Key, Value))
            {
                Value.PropertyChanged += Directory_PropertyChanged;
            }
        }

        private void Directory_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not FileTreeDirectory ChangedDirectory) return;
            switch (e.PropertyName)
            {
                case nameof(ChangedDirectory.IsSelected):
                    if (ChangedDirectory.IsSelected)
                    {
                        // Deselect currently selected directory.
                        if (SelectedItem != null)
                        {
                            SelectedItem.IsSelected = false;
                        }
                        // Set reference to new selected object.
                        SelectedItem = ChangedDirectory;
                    }
                    else
                    {
                        SelectedItem = null;
                    }
                    break;
                case nameof(ChangedDirectory.IsExpanded):
                    // Open child diectories.
                    foreach (var Child in ChangedDirectory.Children)
                    {
                        DirectoryInfo? ChildDirectoryInfo = null;
                        foreach (var Pair in DirectoryMap)
                        {
                            if (Pair.Value == Child)
                            {
                                ChildDirectoryInfo = Pair.Key;
                                break;
                            }
                        }

                        if (ChildDirectoryInfo != null)
                        {
                            DirectoryData.OpenDirectory(ChildDirectoryInfo);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void DirectoryData_AfterDirectoryOpened(object? sender, Model.DirectoryOpenedEventArgs e)
        {
            IEnumerable<FileInfo> MusicFiles = e.OpenedDirectory.GetFiles("*.mp3", SearchOption.TopDirectoryOnly);
            IEnumerable<FileInfo> PlayListFiles = e.OpenedDirectory.GetFiles("*.m3u", SearchOption.TopDirectoryOnly);

            FileTreeDirectory OpenedDirectory = DirectoryMap[e.OpenedDirectory];

            // Add child directories.
            OpenedDirectory.Children.Clear();
            foreach (DirectoryInfo SubDirectory in DirectoryData.GetSubDirectories(e.OpenedDirectory))
            {
                TryAddToDirectoryDictionary(SubDirectory);
                OpenedDirectory.Children.Add(DirectoryMap[SubDirectory]);
            }
            // Add playlists.
            foreach (FileInfo PlaylistFile in PlayListFiles)
            {
                OpenedDirectory.Children.Add(new FileTreePlaylist(PlaylistFile.Name));
            }

            // Add files.
            OpenedDirectory.Files.Clear();
            foreach (var File in MusicFiles)
            {
                OpenedDirectory.Files.Add(File);
            }

            NotifyPropertyChanged(nameof(DirectoryMap));
        }

        private void DirectoryData_AfterDirectoryClosed(object? sender, Model.DirectoryClosedEventArgs e)
        {
        }

        protected virtual void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }

    public class FileTreeItem : INotifyPropertyChanged
    {
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

        public event PropertyChangedEventHandler? PropertyChanged;

        public FileTreeItem(string Name)
        {
            _isSelected = false;
            this.Name = Name;
        }

        protected virtual void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }

    public class FileTreePlaylist : FileTreeItem
    {
        public FileTreePlaylist(string Name) : base(Name)
        {
        }
    }

    public class FileTreeDirectory : FileTreeItem
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

        public ObservableCollection<FileInfo> Files { get; private set; }
        public ObservableCollection<FileTreeItem> Children { get; private set; }

        public FileTreeDirectory(string Name) : base(Name)
        {
            _isExpanded = false;
            Files = new ObservableCollection<FileInfo>();
            Children = new ObservableCollection<FileTreeItem>();
        }
    }
}
