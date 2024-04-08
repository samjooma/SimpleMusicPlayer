using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.IO;
using System.Numerics;
using System.Windows;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace MyMusicPlayer.ViewModel
{
    public class AudioPlayer : INotifyPropertyChanged
    {
        private MediaPlayer Player;

        public ObservableCollection<FileInfo> PlayList { get; private set; }

        private int? _activePlayListIndex;
        public int? ActivePlayListIndex
        {
            get => _activePlayListIndex;
            private set
            {
                if (value != _activePlayListIndex)
                {
                    _activePlayListIndex = value;
                    NotifyPropertyChanged(nameof(ActivePlayListIndex));
                }
            }
        }

        private bool _isPaused;
        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                if (value != _isPaused)
                {
                    _isPaused = value;
                    if (_isPaused)
                    {
                        Player.Pause();
                    }
                    else
                    {
                        Player.Play();
                    }
                    NotifyPropertyChanged(nameof(IsPaused));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public AudioPlayer()
        {
            Player = new MediaPlayer();
            Player.MediaEnded += Player_MediaEnded;
            PlayList = new ObservableCollection<FileInfo>();
            _isPaused = true;
        }

        private void Player_MediaEnded(object? sender, EventArgs e)
        {
            if (ActivePlayListIndex != null)
            {
                ActivePlayListIndex = PlayList.Count > 0 ? (ActivePlayListIndex + 1) % PlayList.Count : null;
            }
        }

        public void PlayFile(FileInfo File)
        {
            Player.Open(new Uri(File.FullName, UriKind.Absolute));
            PlayList.Add(File);
            ActivePlayListIndex = PlayList.Count - 1;
            Player.Play();
            IsPaused = false;
        }

        protected virtual void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
