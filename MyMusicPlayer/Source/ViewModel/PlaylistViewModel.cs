using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Linq;

namespace MyMusicPlayer.ViewModel
{
    public class Playlist
    {
        public string Name { get; set; }
        private List<FileInfo> Songs;
    
        public IEnumerable<string> SongNames { get => Songs.Select(x => x.Name); }
    
        public Playlist(string Name)
        {
            this.Name = Name;
            Songs = new List<FileInfo>();
        }
    
        public void AddSong(FileInfo Song)
        {
            Songs.Add(Song);
        }
    }
}
