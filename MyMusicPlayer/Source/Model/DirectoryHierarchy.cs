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

        public event EventHandler<DirectoryOpenedEventArgs>? AfterDirectoryOpened;
        public event EventHandler<DirectoryClosedEventArgs>? AfterDirectoryClosed;

        public DirectoryHierarchy()
        {
            DirectoryTree = new Dictionary<DirectoryInfo, DirectoryInfo[]>(new DirectoryComparer());
        }

        public DirectoryInfo[] OpenDirectory(DirectoryInfo Directory)
        {
            if (_rootDirectory == null)
            {
                _rootDirectory = Directory;
            }

            DirectoryTree[Directory] = Directory.GetDirectories("*", SearchOption.TopDirectoryOnly);
            AfterDirectoryOpened?.Invoke(this, new DirectoryOpenedEventArgs(Directory));
            return DirectoryTree[Directory];
        }

        public DirectoryInfo[] OpenDirectory(string DirectoryPath)
        {
            return OpenDirectory(new DirectoryInfo(DirectoryPath));
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