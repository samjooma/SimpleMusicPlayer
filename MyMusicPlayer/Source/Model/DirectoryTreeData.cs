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
    public class DirectoryTreeData
    {
        public DirectoryInfo? RootDirectory { get; private set; }
        private Dictionary<DirectoryInfo, DirectoryContent> DirectoryDictionary;

        public event EventHandler<DirectoryAddedEventArgs>? AfterDirectoryAdded;
        public event EventHandler<DirectoryRemovedEventArgs>? AfterDirectoryRemoved;

        public DirectoryTreeData()
        {
            DirectoryDictionary = new Dictionary<DirectoryInfo, DirectoryContent>();
        }

        public void AddDirectory(DirectoryInfo Directory)
        {
            if (DirectoryDictionary.Count == 0)
            {
                RootDirectory = Directory;
            }

            void AddFiles(DirectoryInfo d)
            {
                var AllFiles = d.GetFiles("*", SearchOption.TopDirectoryOnly);
                string[] AudioExtensions = [".mp3"];
                string[] PlaylistExtensions = [".m3u"];
                DirectoryDictionary[d] = new DirectoryContent(
                    d.GetDirectories("*", SearchOption.TopDirectoryOnly),
                    Array.FindAll(AllFiles, x => AudioExtensions.Contains(x.Extension)),
                    Array.FindAll(AllFiles, x => PlaylistExtensions.Contains(x.Extension))
                );
            }

            AddFiles(Directory);
            foreach (DirectoryInfo SubDirectory in DirectoryDictionary[Directory].SubDirectories)
            {
                AddFiles(SubDirectory);
            }

            AfterDirectoryAdded?.Invoke(this, new DirectoryAddedEventArgs(Directory));
        }

        public void RemoveDirectory(DirectoryInfo Directory)
        {
            if (Directory == RootDirectory)
            {
                throw new ArgumentException(); //TODO: Better exception.
            }

            // Close children recursively.
            foreach (var Subdirectory in DirectoryDictionary[Directory].SubDirectories)
            {
                if (DirectoryDictionary.ContainsKey(Subdirectory))
                {
                    RemoveDirectory(Subdirectory);
                }
            }
            DirectoryDictionary.Remove(Directory);
            AfterDirectoryRemoved?.Invoke(this, new DirectoryRemovedEventArgs(Directory));
        }

        public void RemoveSubDirectories(DirectoryInfo Directory)
        {
            foreach (var SubDirectory in GetSubDirectories(Directory))
            {
                RemoveDirectory(SubDirectory);
            }
        }

        public void Clear()
        {
            DirectoryDictionary.Clear();
            RootDirectory = null;
        }

        public bool ContainsKey(DirectoryInfo Directory)
        {
            return DirectoryDictionary.ContainsKey(Directory);
        }
        
        public DirectoryInfo[] GetSubDirectories(DirectoryInfo Directory)
        {
            return DirectoryDictionary[Directory].SubDirectories;
        }

        public FileInfo[] GetAudioFiles(DirectoryInfo Directory)
        {
            return DirectoryDictionary[Directory].AudioFiles;
        }

        public FileInfo[] GetPlaylistFiles(DirectoryInfo Directory)
        {
            return DirectoryDictionary[Directory].Playlists;
        }

        public DirectoryInfo? GetParent(DirectoryInfo Directory)
        {
            try
            {
                return DirectoryDictionary.First(x => x.Value.SubDirectories.Contains(Directory)).Key;
            }
            catch (InvalidOperationException)
            {
                return null;
            }   
        }

        public DirectoryInfo[] GetAllOpenDirectories()
        {
            return DirectoryDictionary.Keys.ToArray();
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

    public class DirectoryAddedEventArgs(DirectoryInfo Directory) : EventArgs
    {
        public DirectoryInfo OpenedDirectory = Directory;
    }

    public class DirectoryRemovedEventArgs(DirectoryInfo Directory) : EventArgs
    {
        public DirectoryInfo ClosedDirectory = Directory;
    }
}