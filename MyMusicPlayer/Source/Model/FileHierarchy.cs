using MS.WindowsAPICodePack.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMusicPlayer.Model
{
    public class DirectoryContentsChangedEventArgs(DirectoryInfo Directory) : EventArgs
    {
        public DirectoryInfo Directory = Directory;
    }

    public class FileHierarchy
    {
        public DirectoryInfo RootDirectory { get; private set; }
        private Dictionary<DirectoryInfo, DirectoryInfo[]> DirectoryMap;
        //private Dictionary<DirectoryInfo, List<FileInfo>?> FileMap;

        public event EventHandler<DirectoryContentsChangedEventArgs> DirectoryContentChanged;

        public FileHierarchy(string FilePath)
        {
            RootDirectory = new DirectoryInfo(FilePath);
            DirectoryMap = new Dictionary<DirectoryInfo, DirectoryInfo[]>();
            //FileMap = new Dictionary<DirectoryInfo, List<FileInfo>?>();
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
            DirectoryContentChanged?.Invoke(this, new DirectoryContentsChangedEventArgs(Directory));
            return DirectoryMap[Directory];
        }

        public void CloseDirectory(DirectoryInfo Directory)
        {
            DirectoryMap.Remove(Directory);
            DirectoryContentChanged?.Invoke(this, new DirectoryContentsChangedEventArgs(Directory));
        }
    }
}