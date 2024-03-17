using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMusicPlayer.Model
{
    public class FileHierarchy
    {
        public DirectoryInfo RootDirectory { get; private set; }
        private Dictionary<DirectoryInfo, DirectoryInfo[]> DirectoryMap;
        //private Dictionary<DirectoryInfo, List<FileInfo>?> FileMap;

        public FileHierarchy(string FilePath)
        {
            RootDirectory = new DirectoryInfo(FilePath);
            DirectoryMap = new Dictionary<DirectoryInfo, DirectoryInfo[]>();
            //FileMap = new Dictionary<DirectoryInfo, List<FileInfo>?>();
        }

        private void UpdateSubDirectories(DirectoryInfo Directory)
        {
            DirectoryMap[Directory] = Directory.GetDirectories("*", SearchOption.TopDirectoryOnly);
        }

        public DirectoryInfo[] OpenSubDirectories(DirectoryInfo Directory)
        {
            UpdateSubDirectories(Directory);
            return DirectoryMap[Directory];
        }

        public void CloseSubDirectories(DirectoryInfo Directory)
        {
            DirectoryMap.Remove(Directory);
        }
    }
}