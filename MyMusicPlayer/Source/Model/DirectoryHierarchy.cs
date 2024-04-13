using MS.WindowsAPICodePack.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace MyMusicPlayer.Model
{
    public class DirectoryHierarchy
    {
        private DirectoryInfo? _rootDirectory;
        public DirectoryInfo RootDirectory
        {
            get
            {
                if (_rootDirectory == null) { throw new ArgumentNullException(); }
                return _rootDirectory;
            }
        }
        private Dictionary<DirectoryInfo, DirectoryContent> DirectoryTree;

        public event EventHandler<DirectoryOpenedEventArgs>? AfterDirectoryOpened;
        public event EventHandler<DirectoryClosedEventArgs>? AfterDirectoryClosed;

        public DirectoryHierarchy()
        {
            DirectoryTree = new Dictionary<DirectoryInfo, DirectoryContent>();
        }

        public void OpenDirectory(DirectoryInfo Directory)
        {
            if (_rootDirectory == null)
            {
                _rootDirectory = Directory;
            }

            void GetFiles(DirectoryInfo d)
            {
                var AllFiles = d.GetFiles("*", SearchOption.TopDirectoryOnly);
                string[] AudioExtensions = [".mp3"];
                string[] PlaylistExtensions = [".m3u"];
                DirectoryTree[d] = new DirectoryContent(
                    d.GetDirectories("*", SearchOption.TopDirectoryOnly),
                    Array.FindAll(AllFiles, x => AudioExtensions.Contains(x.Extension)),
                    Array.FindAll(AllFiles, x => PlaylistExtensions.Contains(x.Extension))
                );
            }

            GetFiles(Directory);
            foreach (DirectoryInfo SubDirectory in DirectoryTree[Directory].SubDirectories)
            {
                GetFiles(SubDirectory);
            }

            AfterDirectoryOpened?.Invoke(this, new DirectoryOpenedEventArgs(Directory));
        }

        public void CloseDirectory(DirectoryInfo Directory)
        {
            if (Directory == RootDirectory)
            {
                throw new ArgumentException(); //TODO: Better exception.
            }

            // Close children recursively.
            foreach (var Subdirectory in DirectoryTree[Directory].SubDirectories)
            {
                if (DirectoryTree.ContainsKey(Subdirectory))
                {
                    CloseDirectory(Subdirectory);
                }
            }
            DirectoryTree.Remove(Directory);
            AfterDirectoryClosed?.Invoke(this, new DirectoryClosedEventArgs(Directory));
        }

        public void CloseSubDirectories(DirectoryInfo Directory)
        {
            foreach (var SubDirectory in GetSubDirectories(Directory))
            {
                CloseDirectory(SubDirectory);
            }
        }

        public bool IsDirectoryOpen(DirectoryInfo Directory)
        {
            return DirectoryTree.ContainsKey(Directory);
        }
        
        public DirectoryInfo[] GetSubDirectories(DirectoryInfo Directory)
        {
            return DirectoryTree[Directory].SubDirectories;
        }

        public FileInfo[] GetAudioFiles(DirectoryInfo Directory)
        {
            return DirectoryTree[Directory].AudioFiles;
        }

        public FileInfo[] GetPlaylistFiles(DirectoryInfo Directory)
        {
            return DirectoryTree[Directory].Playlists;
        }

        public DirectoryInfo? GetParent(DirectoryInfo Directory)
        {
            try
            {
                return DirectoryTree.First(x => x.Value.SubDirectories.Contains(Directory)).Key;
            }
            catch (InvalidOperationException)
            {
                return null;
            }   
        }

        public DirectoryInfo[] GetAllOpenDirectories()
        {
            return DirectoryTree.Keys.ToArray();
        }
    }

    public class DirectoryContent(DirectoryInfo[] SubDirectories, FileInfo[] AudioFiles, FileInfo[] Playlists)
    {
        public DirectoryInfo[] SubDirectories = SubDirectories;
        public FileInfo[] AudioFiles = AudioFiles;
        public FileInfo[] Playlists = Playlists;
    }

    public class DirectoryComparer : IEqualityComparer<DirectoryInfo>
    {
        public bool Equals(DirectoryInfo? a, DirectoryInfo? b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;
            return a.FullName == b.FullName;
        }

        public int GetHashCode([DisallowNull] DirectoryInfo obj)
        {
            return obj.FullName.GetHashCode();
        }
    }

    public class DirectoryOpenedEventArgs(DirectoryInfo Directory) : EventArgs
    {
        public DirectoryInfo OpenedDirectory = Directory;
    }

    public class DirectoryClosedEventArgs(DirectoryInfo Directory) : EventArgs
    {
        public DirectoryInfo ClosedDirectory = Directory;
    }
}