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

        private FileInfo? _playingFile;
        public FileInfo? PlayingFile
        {
            private set
            {
                if (value != _playingFile)
                {
                    _playingFile = value;
                    NotifyPropertyChanged(nameof(PlayingFile));
                }
            }
            get
            {
                return _playingFile;
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
            _isPaused = true;
        }

        public void PlayFile(FileInfo File)
        {
            Player.Open(new Uri(File.FullName, UriKind.Absolute));
            Player.Play();
            IsPaused = false;
            PlayingFile = File;
        }

        public void Stop()
        {
            Player.Stop();
            IsPaused = true;
            PlayingFile = null;
        }

        protected virtual void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
