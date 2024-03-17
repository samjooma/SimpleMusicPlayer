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
    public class DirectoryContentsChangedEventArgs(DirectoryInfo Directory, DirectoryInfo[] NewSubDirectories) : EventArgs
    {
        public DirectoryInfo Directory = Directory;
        public DirectoryInfo[] NewSubDirectories = NewSubDirectories;
    }

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
        public DirectoryInfo RootDirectory { get; private set; }
        private Dictionary<DirectoryInfo, DirectoryInfo[]> DirectoryMap;
        //private Dictionary<DirectoryInfo, List<FileInfo>?> FileMap;

        public event EventHandler<DirectoryContentsChangedEventArgs> DirectoryContentChanged;
        public event EventHandler<DirectoryOpenedEventArgs> AfterDirectoryOpened;
        public event EventHandler<DirectoryClosedEventArgs> AfterDirectoryClosed;

        public FileHierarchy(string FilePath)
        {
            RootDirectory = new DirectoryInfo(FilePath);
            DirectoryMap = new Dictionary<DirectoryInfo, DirectoryInfo[]>();
            //FileMap = new Dictionary<DirectoryInfo, List<FileInfo>?>();
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

        public DirectoryInfo[] OpenDirectory(DirectoryInfo Directory)
        {
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