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
    public class DirectoryOpenedEventArgs(DirectoryInfo Directory, DirectoryInfo[] SubDirectories) : EventArgs
    {
        public DirectoryInfo Directory = Directory;
        public DirectoryInfo[] SubDirectories = SubDirectories;
    }

    public class DirectoryClosedEventArgs(DirectoryInfo Directory) : EventArgs
    {
        public DirectoryInfo Directory = Directory;
    }

    public class FileHierarchy
    {
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

        private DirectoryInfo? _rootDirectory;
        public DirectoryInfo RootDirectory {
            get
            {
                if (_rootDirectory == null)
                {
                    throw new ArgumentNullException();
                }
                return _rootDirectory;
            }
        }
        private Dictionary<DirectoryInfo, DirectoryInfo[]> DirectoryMap;

        public event EventHandler<DirectoryOpenedEventArgs>? AfterDirectoryOpened;
        public event EventHandler<DirectoryClosedEventArgs>? AfterDirectoryClosed;

        public FileHierarchy()
        {
            DirectoryMap = new Dictionary<DirectoryInfo, DirectoryInfo[]>(new DirectoryComparer());
        }

        public DirectoryInfo[] OpenDirectory(DirectoryInfo Directory)
        {
            if (_rootDirectory == null)
            {
                _rootDirectory = Directory;
            }

            DirectoryMap[Directory] = Directory.GetDirectories("*", SearchOption.TopDirectoryOnly);
            AfterDirectoryOpened?.Invoke(this, new DirectoryOpenedEventArgs(Directory, DirectoryMap[Directory]));
            return DirectoryMap[Directory];
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
            foreach (var Subdirectory in DirectoryMap[Directory])
            {
                if (DirectoryMap.ContainsKey(Subdirectory))
                {
                    CloseDirectory(Subdirectory);
                }
            }
            DirectoryMap.Remove(Directory);
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
            return DirectoryMap[Directory];
        }

        public DirectoryInfo? GetParent(DirectoryInfo Directory)
        {
            try
            {
                return DirectoryMap.First(x => x.Value.Contains(Directory)).Key;
            }
            catch (InvalidOperationException)
            {
                return null;
            }   
        }

        public DirectoryInfo[] GetAllOpenDirectories()
        {
            return DirectoryMap.Keys.ToArray();
        }
    }
}