using SimpleMusicPlayer.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;

namespace SimpleMusicPlayer.ViewModel
{
    public class SongQueue : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private ObservableCollection<SongQueueItem> _songQueueItems;
        public ReadOnlyObservableCollection<SongQueueItem> SongQueueItems { get; private set; }

        private SongQueueItem? _activeSong;
        public SongQueueItem? ActiveSong
        {
            get => _activeSong;
            private set
            {
                if (value != _activeSong)
                {
                    if (_activeSong != null) _activeSong.IsSongActive = false;
                    _activeSong = value;
                    if (_activeSong != null) _activeSong.IsSongActive = true;
                    NotifyPropertyChanged(nameof(ActiveSong));
                }
            }
        }

        private int? _activeSongIndex;
        public int? ActiveSongIndex {
            get => _activeSongIndex;
            set
            {
                if (value != _activeSongIndex)
                {
                    _activeSongIndex = value;
                    ActiveSong = _activeSongIndex != null ? SongQueueItems[_activeSongIndex.Value] : null;
                    NotifyPropertyChanged(nameof(ActiveSongIndex));
                }
            }
        }

        public SongQueue()
        {
            _songQueueItems = new ObservableCollection<SongQueueItem>();
            _songQueueItems.CollectionChanged += FileList_CollectionChanged;
            SongQueueItems = new ReadOnlyObservableCollection<SongQueueItem>(_songQueueItems);
            _activeSongIndex = null;
            _activeSong = null;
        }

        public void AddSong(FileInfo File)
        {
            _songQueueItems.Add(new SongQueueItem(File, false));
        }

        public void InsertSong(int Index, FileInfo File)
        {
            _songQueueItems.Insert(Index, new SongQueueItem(File, false));
        }

        public void RemoveSongAt(int Index)
        {
            _songQueueItems.RemoveAt(Index);
        }

        public void Clear()
        {
            _songQueueItems.Clear();
        }

        public void MoveSong(int FromIndex, int ToIndex)
        {
            _songQueueItems.Move(FromIndex, ToIndex);
        }

        public void NextSong()
        {
            if (ActiveSongIndex != null)
            {
                SetActiveIndexWrapped(ActiveSongIndex.Value + 1);
            }
            else
            {
                ActiveSongIndex = null;
            }
        }

        public void PreviousSong()
        {
            if (ActiveSongIndex != null)
            {
                SetActiveIndexWrapped(ActiveSongIndex.Value - 1);
            }
            else
            {
                ActiveSongIndex = null;
            }
        }

        protected virtual void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        private void SetActiveIndexWrapped(int Index)
        {
            int Count = SongQueueItems.Count;
            int? WrappedIndex = (Index % Count + Count) % Count; // Keep index in range [0, Count[.
            ActiveSongIndex = WrappedIndex;
        }

        private void FileList_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            int FoundIndex = SongQueueItems.ToList().FindIndex(x => ReferenceEquals(x, ActiveSong));
            ActiveSongIndex = FoundIndex > -1 ? FoundIndex : null;
        }
    }
}
