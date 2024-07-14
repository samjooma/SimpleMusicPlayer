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

namespace SimpleMusicPlayer.ViewModel
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

        private TimeSpan _previousTime;

        public TimeSpan CurrentTime
        {
            get => Player.Position;
            set
            {
                if (value != Player.Position)
                {
                    Player.Position = value;
                    NotifyPropertyChanged(nameof(CurrentTime));
                }
            }
        }

        private TimeSpan _duration;
        public TimeSpan Duration
        {
            get => _duration;
            private set
            {
                if (value != Duration)
                {
                    _duration = value;
                    NotifyPropertyChanged(nameof(Duration));
                }
            }
        }

        public double Volume
        {
            get => Player.Volume;
            set
            {
                if (value != Volume)
                {
                    Player.Volume = value;
                    NotifyPropertyChanged(nameof(Volume));
                }
            }
        }

        private System.Windows.Threading.DispatcherTimer Timer;

        public event PropertyChangedEventHandler? PropertyChanged;

        public AudioPlayer()
        {
            Timer = new System.Windows.Threading.DispatcherTimer(System.Windows.Threading.DispatcherPriority.Render);
            Timer.Interval = TimeSpan.FromMilliseconds(1);
            Timer.Tick += UpdateCurrentTime;
            Timer.Start();
            Player = new MediaPlayer();
            Player.MediaOpened += Player_MediaOpened;
            _isPaused = true;
            _duration = TimeSpan.Zero;
            _previousTime = TimeSpan.Zero;
        }

        public void OpenFile(FileInfo File)
        {
            Player.Open(new Uri(File.FullName, UriKind.Absolute));
        }

        private void Player_MediaOpened(object? Sender, EventArgs e)
        {
            OpenedAudioFile = new FileInfo(Player.Source.AbsolutePath);
            Duration = Player.NaturalDuration.TimeSpan;
            _previousTime = TimeSpan.Zero;
            Play();
        }

        public void CloseFile()
        {
            Player.Close();
            OpenedAudioFile = null;
            IsPaused = true;
            Duration = TimeSpan.Zero;
            _previousTime = TimeSpan.Zero;
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
            if (!IsPaused)
            {
                int TotalSeconds = (int)Math.Floor(Player.Position.TotalSeconds);
                int PreviousTotalSeconds = (int)Math.Floor(_previousTime.TotalSeconds);
                if (TotalSeconds != PreviousTotalSeconds)
                {
                    _previousTime = Player.Position;
                    NotifyPropertyChanged(nameof(CurrentTime));
                }
            }
        }
    }
}
