using MS.WindowsAPICodePack.Internal;
using System;
using System.Collections.Generic;
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
            DirectoryMap = new Dictionary<DirectoryInfo, DirectoryInfo[]>();
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

        public DirectoryInfo[] OpenDirectory(string DirectoryPath)
        {
            return OpenDirectory(new DirectoryInfo(DirectoryPath));
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

        public void CloseDirectory(DirectoryInfo Directory)
        {
            // Close children.
            foreach (var SubDirectory in DirectoryMap[Directory])
            {
                if (DirectoryMap.ContainsKey(SubDirectory))
                {
                    CloseDirectory(SubDirectory);
                }
            }
            DirectoryMap.Remove(Directory);
            AfterDirectoryClosed?.Invoke(this, new DirectoryClosedEventArgs(Directory));
        }
    }
}