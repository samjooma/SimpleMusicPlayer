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
        private Dictionary<DirectoryInfo, DirectoryNode> DirectoryNodesDictionary { get; set; }
        private TreeNode? _selectedItem;
        public TreeNode? SelectedItem
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

        public DirectoryNode RootDirectory { get => DirectoryNodesDictionary[DirectoryData.RootDirectory]; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public DirectoryTree()
        {
            DirectoryNodesDictionary = new Dictionary<DirectoryInfo, DirectoryNode>(new Model.DirectoryComparer());
            DirectoryData = new Model.DirectoryHierarchy();
        }

        public void SetRootDirectory(string RootDirectoryPath)
        {
            DirectoryNodesDictionary.Clear();
            DirectoryData = new Model.DirectoryHierarchy();

            var RootDirectory = new DirectoryInfo(RootDirectoryPath);
            GetOrCreateDirectoryNode(RootDirectory);

            DirectoryData.AfterDirectoryOpened += DirectoryData_AfterDirectoryOpened;
            DirectoryData.AfterDirectoryClosed += DirectoryData_AfterDirectoryClosed;
            DirectoryData.OpenDirectory(RootDirectory);
        }

        //TODO: This function is bad.
        public DirectoryNode[] GetAllDirectories()
        {
            return DirectoryNodesDictionary.Values.ToArray();
        }

        private DirectoryNode GetOrCreateDirectoryNode(DirectoryInfo Key)
        {
            var Value = new DirectoryNode(Key.Name);
            if (DirectoryNodesDictionary.TryAdd(Key, Value))
            {
                Value.PropertyChanged += TreeNode_PropertyChanged;
            }
            return DirectoryNodesDictionary[Key];
        }

        private void TreeNode_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not DirectoryNode ChangedNode) return;
            switch (e.PropertyName)
            {
                case nameof(ChangedNode.IsSelected):
                    if (ChangedNode.IsSelected)
                    {
                        // Deselect currently selected directory.
                        if (SelectedItem != null)
                        {
                            SelectedItem.IsSelected = false;
                        }
                        // Set reference to new selected object.
                        SelectedItem = ChangedNode;
                    }
                    else
                    {
                        SelectedItem = null;
                    }
                    break;
                case nameof(ChangedNode.IsExpanded):
                    // Open child diectories.
                    foreach (var Child in ChangedNode.Children)
                    {
                        DirectoryInfo? ChildDirectoryInfo = null;
                        foreach (var Pair in DirectoryNodesDictionary)
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
            DirectoryNode OpenedNode = DirectoryNodesDictionary[e.OpenedDirectory];

            //
            // Add child nodes.
            //

            var PlayListFiles = new ObservableCollection<FileInfo>(DirectoryData.GetPlaylistFiles(e.OpenedDirectory));
            OpenedNode.Children.Clear();
            // Add directories.
            foreach (DirectoryInfo SubDirectory in DirectoryData.GetSubDirectories(e.OpenedDirectory))
            {
                OpenedNode.Children.Add(GetOrCreateDirectoryNode(SubDirectory));
            }
            // Add playlists.
            foreach (FileInfo PlaylistFile in PlayListFiles)
            {
                OpenedNode.Children.Add(new PlaylistNode(PlaylistFile.Name));
            }

            //
            // Add files.
            //

            OpenedNode.AudioFiles = new ObservableCollection<FileInfo>(DirectoryData.GetAudioFiles(e.OpenedDirectory));

            NotifyPropertyChanged(nameof(DirectoryNodesDictionary));
        }

        private void DirectoryData_AfterDirectoryClosed(object? sender, Model.DirectoryClosedEventArgs e)
        {
        }

        protected virtual void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }

    public class TreeNode : INotifyPropertyChanged
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
        public ObservableCollection<FileInfo> AudioFiles { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public TreeNode(string Name)
        {
            _isSelected = false;
            this.Name = Name;
            AudioFiles = new ObservableCollection<FileInfo>();
        }

        protected virtual void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }

    public class PlaylistNode : TreeNode
    {
        public PlaylistNode(string Name) : base(Name)
        {
        }
    }

    public class DirectoryNode : TreeNode
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

        public ObservableCollection<TreeNode> Children { get; set; }

        public DirectoryNode(string Name) : base(Name)
        {
            _isExpanded = false;
            Children = new ObservableCollection<TreeNode>();
        }
    }
}
