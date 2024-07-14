using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMusicPlayer.Model
{
    public class PlaylistParser
    {
        public static List<FileInfo> ReadPLaylist_M3U(FileInfo Playlist)
        {
            var Result = new List<FileInfo>();
            using (FileStream s = Playlist.OpenRead())
            using (StreamReader Reader = new StreamReader(s))
            {
                while (!Reader.EndOfStream)
                {
                    string? Line = Reader.ReadLine();
                    if (Line != null)
                    {
                        Result.Add(new FileInfo(Line));
                    }
                }
            }
            return Result;
        }
    }
}
