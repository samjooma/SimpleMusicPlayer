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
        public event PropertyChangedEventHandler? PropertyChanged;

        private Model.DirectoryTreeData DirectoryData { get; set; }
        private Dictionary<DirectoryInfo, DirectoryNode> DirectoryNodesDictionary { get; set; }
        private Node? _selectedDirectory;
        public Node? SelectedDirectory
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

        public DirectoryNode? RootDirectory
        {
            get => DirectoryData.RootDirectory != null ? DirectoryNodesDictionary[DirectoryData.RootDirectory] : null;
        }

        public DirectoryTree()
        {
            DirectoryNodesDictionary = new Dictionary<DirectoryInfo, DirectoryNode>(new Model.DirectoryComparer());
            DirectoryData = new Model.DirectoryTreeData();

            DirectoryNodesDictionary.Clear();
            DirectoryData = new Model.DirectoryTreeData();

            DirectoryData.AfterDirectoryAdded += DirectoryData_AfterDirectoryAdded;
            DirectoryData.AfterDirectoryRemoved += DirectoryData_AfterDirectoryRemoved;
        }

        public void AddDirectory(DirectoryInfo Directory)
        {
            DirectoryData.AddDirectory(Directory);
        }

        public void Clear()
        {
            DirectoryData.Clear();
            DirectoryNodesDictionary.Clear();
            _selectedDirectory = null;
        }

        private DirectoryNode CreateDirectoryNodeRecursive(DirectoryInfo Directory)
        {
            // Get or create node.
            DirectoryNode? Node;
            if (!DirectoryNodesDictionary.TryGetValue(Directory, out Node))
            {
                Node = new DirectoryNode(Directory);
                DirectoryNodesDictionary[Directory] = Node;
                Node.IsSelectedChanged += TreeNode_IsSelectedChanged;
                Node.IsExpandedChanged += DirectoryNode_IsExpandedChanged;
            }

            Node.Children.Clear();
            if (DirectoryData.ContainsKey(Directory))
            {
                // Add files.
                Node.AudioFiles = new ObservableCollection<FileInfo>(DirectoryData.GetAudioFiles(Directory));

                // Add playlists.
                foreach (FileInfo PlaylistFile in DirectoryData.GetPlaylistFiles(Directory))
                {
                    var Child = new PlaylistNode(PlaylistFile.Name);
                    Child.IsSelectedChanged += TreeNode_IsSelectedChanged;
                    Child.AudioFiles = new ObservableCollection<FileInfo>(Model.PlaylistParser.ReadPLaylist_M3U(PlaylistFile));
                    Node.Children.Add(Child);
                }

                // Add child directories.
                foreach (DirectoryInfo SubDirectory in DirectoryData.GetSubDirectories(Directory))
                {
                    Node.Children.Add(CreateDirectoryNodeRecursive(SubDirectory));
                }
            }

            return Node;
        }

        private void RemoveDirectoryNodeRecursive(DirectoryInfo Directory)
        {
            DirectoryNode? Node;
            if (DirectoryNodesDictionary.TryGetValue(Directory, out Node))
            {
                foreach (var Child in Node.Children.OfType<DirectoryNode>())
                {
                    RemoveDirectoryNodeRecursive(Child.Info);
                }
                DirectoryNodesDictionary.Remove(Directory);
            }
        }

        private void TreeNode_IsSelectedChanged(object? Sender, EventArgs e)
        {
            if (Sender is not Node ChangedNode) throw new ArgumentException();
            if (ChangedNode.IsSelected)
            {
                // Deselect currently selected directory.
                if (SelectedDirectory != null)
                {
                    SelectedDirectory.IsSelected = false;
                }
                // Set reference to new selected object.
                SelectedDirectory = ChangedNode;
            }
            else
            {
                SelectedDirectory = null;
            }
        }

        private void DirectoryNode_IsExpandedChanged(object? Sender, EventArgs e)
        {
            if (Sender is not DirectoryNode ChangedNode) throw new ArgumentException();
            if (ChangedNode.IsExpanded)
            {
                // Open child directories.
                foreach (DirectoryNode Child in ChangedNode.Children.OfType<DirectoryNode>())
                {
                    DirectoryData.AddDirectory(Child.Info);
                }
            }
        }

        private void DirectoryData_AfterDirectoryAdded(object? Sender, Model.DirectoryAddedEventArgs e)
        {
            CreateDirectoryNodeRecursive(e.OpenedDirectory);
            if (e.OpenedDirectory == DirectoryData.RootDirectory)
            {
                NotifyPropertyChanged(nameof(RootDirectory));
            }
        }

        private void DirectoryData_AfterDirectoryRemoved(object? Sender, Model.DirectoryRemovedEventArgs e)
        {
            RemoveDirectoryNodeRecursive(e.ClosedDirectory);
        }

        protected virtual void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }

    public class Node
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
                    IsSelectedChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public string Name { get; private set; }
        public ObservableCollection<FileInfo> AudioFiles { get; set; }

        public event EventHandler<EventArgs>? IsSelectedChanged;

        public Node(string Name)
        {
            _isSelected = false;
            this.Name = Name;
            AudioFiles = new ObservableCollection<FileInfo>();
        }
    }

    public class PlaylistNode : Node
    {
        public PlaylistNode(string Name) : base(Name)
        {
        }
    }

    public class DirectoryNode : Node
    {
        internal DirectoryInfo Info { get; set; }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    IsExpandedChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public ObservableCollection<Node> Children { get; set; }

        public event EventHandler<EventArgs>? IsExpandedChanged;

        public DirectoryNode(DirectoryInfo Info) : base(Info.Name)
        {
            this.Info = Info;
            _isExpanded = false;
            Children = new ObservableCollection<Node>();
        }
    }
}
