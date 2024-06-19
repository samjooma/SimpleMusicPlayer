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

        FileInfo? _openedAudioFile;
        public FileInfo? OpenedAudioFile
        {
            get => _openedAudioFile;
            set
            {
                if (value != _openedAudioFile)
                {
                    _openedAudioFile = value;
                    NotifyPropertyChanged(nameof(OpenedAudioFile));
                }
            }
        }

        private bool _isPaused;
        public bool IsPaused
        {
            get => _isPaused;
            private set
            {
                if (value != _isPaused)
                {
                    _isPaused = value;
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

        public void OpenFile(FileInfo File)
        {
            Player.Open(new Uri(File.FullName, UriKind.Absolute));
            OpenedAudioFile = File;
            IsPaused = true;
        }

        public void CloseFile()
        {
            Player.Close();
            OpenedAudioFile = null;
            IsPaused = true;
        }

        public void Play()
        {
            if (Player.HasAudio)
            {
                Player.Play();
                IsPaused = false;
            }
        }

        public void Pause()
        {
            Player.Pause();
            IsPaused = true;
        }

        public void TogglePause()
        {
            if (IsPaused)
            {
                Play();
            }
            else
            {
                Pause();
            }
        }

        public void Stop()
        {
            Player.Stop();
            IsPaused = true;
        }

        protected virtual void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
