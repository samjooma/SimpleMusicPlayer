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
            UpdateSubDirectories(RootDirectory);
        }

        public void ClearSubDirectories(DirectoryInfo Directory)
        {
            DirectoryMap.Remove(Directory);
        }

        public void UpdateSubDirectories(DirectoryInfo Directory)
        {
            if (!DirectoryMap.ContainsKey(Directory))
            {
                DirectoryMap[Directory] = Directory.GetDirectories("*", SearchOption.TopDirectoryOnly);
            }
        }

        public DirectoryInfo[]? GetSubDirectories(DirectoryInfo Directory)
        {
            DirectoryInfo[]? Result;
            DirectoryMap.TryGetValue(Directory, out Result);
            return Result;
        }
    }
}