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

        private TimeSpan _currentTime;
        public TimeSpan CurrentTime { get => _currentTime; }
        public Duration Duration { get => Player.NaturalDuration; }

        private System.Windows.Threading.DispatcherTimer Timer;

        public event PropertyChangedEventHandler? PropertyChanged;

        public AudioPlayer()
        {
            Timer = new System.Windows.Threading.DispatcherTimer(System.Windows.Threading.DispatcherPriority.Render);
            Timer.Interval = TimeSpan.FromMilliseconds(1);
            Timer.Tick += UpdateCurrentTime;
            Timer.Start();
            Player = new MediaPlayer();
            _isPaused = true;
            _currentTime = TimeSpan.Zero;
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

        private void UpdateCurrentTime(object? Sender, EventArgs e)
        {
            int FlooredSeconds = (int)Math.Floor(Player.Position.TotalSeconds);
            if (FlooredSeconds != (int)_currentTime.TotalSeconds)
            {
                _currentTime = TimeSpan.FromSeconds(FlooredSeconds);
                NotifyPropertyChanged(nameof(CurrentTime));
            }
        }
    }
}
