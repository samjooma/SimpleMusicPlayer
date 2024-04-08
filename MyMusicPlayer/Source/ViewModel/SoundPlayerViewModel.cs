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

namespace MyMusicPlayer.ViewModel
{
    public class SoundPlayer : INotifyPropertyChanged
    {
        private MediaPlayer Player;
        private FileInfo? _activeFile;
        private FileInfo? ActiveFile // Note: This is private on purpose.
        {
            get => _activeFile;
            set
            {
                if (value != _activeFile)
                {
                    _activeFile = value;
                    NotifyPropertyChanged(nameof(ActiveFileName));
                }
            }
        }
        public string ActiveFileName { get => ActiveFile != null ? ActiveFile.Name : ""; }

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

        public SoundPlayer()
        {
            Player = new MediaPlayer();
            Player.MediaEnded += Player_MediaEnded;
            _isPaused = true;
        }

        private void Player_MediaEnded(object? sender, EventArgs e)
        {
            ActiveFile = null;
        }

        public void PlayFile(FileInfo File)
        {
            Player.Open(new Uri(File.FullName, UriKind.Absolute));
            ActiveFile = File;
            Player.Play();
            IsPaused = false;
        }

        protected virtual void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
