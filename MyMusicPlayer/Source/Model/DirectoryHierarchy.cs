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
        private Dictionary<DirectoryInfo, DirectoryInfo[]> DirectoryTree;
        private Dictionary<DirectoryInfo, FileInfo[]> AudioFiles;
        private Dictionary<DirectoryInfo, FileInfo[]> PlaylistFiles;

        public event EventHandler<DirectoryOpenedEventArgs>? AfterDirectoryOpened;
        public event EventHandler<DirectoryClosedEventArgs>? AfterDirectoryClosed;

        public DirectoryHierarchy()
        {
            DirectoryTree = new Dictionary<DirectoryInfo, DirectoryInfo[]>(new DirectoryComparer());
            AudioFiles = new Dictionary<DirectoryInfo, FileInfo[]>(new DirectoryComparer());
            PlaylistFiles = new Dictionary<DirectoryInfo, FileInfo[]>(new DirectoryComparer());
        }

        public void OpenDirectory(DirectoryInfo Directory)
        {
            if (_rootDirectory == null)
            {
                _rootDirectory = Directory;
            }

            DirectoryTree[Directory] = Directory.GetDirectories("*", SearchOption.TopDirectoryOnly);
            var AllFiles = Directory.GetFiles("*", SearchOption.TopDirectoryOnly);

            string[] AudioExtensions = [".mp3"];
            AudioFiles[Directory] = Array.FindAll(AllFiles, x => AudioExtensions.Contains(x.Extension));
            string[] PlaylistExtensions = [".m3u"];
            PlaylistFiles[Directory] = Array.FindAll(AllFiles, x => PlaylistExtensions.Contains(x.Extension));

            AfterDirectoryOpened?.Invoke(this, new DirectoryOpenedEventArgs(Directory));
        }

        public void CloseDirectory(DirectoryInfo Directory)
        {
            if (Directory == RootDirectory)
            {
                throw new ArgumentException(); //TODO: Better exception.
            }

            // Close children recursively.
            foreach (var Subdirectory in DirectoryTree[Directory])
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
        
        public DirectoryInfo[] GetSubDirectories(DirectoryInfo Directory)
        {
            return DirectoryTree[Directory];
        }

        public FileInfo[] GetAudioFiles(DirectoryInfo Directory)
        {
            return AudioFiles[Directory];
        }

        public FileInfo[] GetPlaylistFiles(DirectoryInfo Directory)
        {
            return PlaylistFiles[Directory];
        }

        public DirectoryInfo? GetParent(DirectoryInfo Directory)
        {
            try
            {
                return DirectoryTree.First(x => x.Value.Contains(Directory)).Key;
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
        public DirectoryInfo Directory = Directory;
    }
}