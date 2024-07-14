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

        private ObservableCollection<SongQueueItem> _fileList;
        public ReadOnlyObservableCollection<SongQueueItem> FileList { get; private set; }

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
                    ActiveSong = _activeSongIndex != null ? FileList[_activeSongIndex.Value] : null;
                    NotifyPropertyChanged(nameof(ActiveSongIndex));
                }
            }
        }

        public SongQueue()
        {
            _fileList = new ObservableCollection<SongQueueItem>();
            _fileList.CollectionChanged += FileList_CollectionChanged;
            FileList = new ReadOnlyObservableCollection<SongQueueItem>(_fileList);
            _activeSongIndex = null;
            _activeSong = null;
        }

        public void AddSong(SongQueueItem Item)
        {
            _fileList.Add(Item);
        }

        public void InsertSong(int Index, SongQueueItem Item)
        {
            _fileList.Insert(Index, Item);
        }

        public void RemoveSongAt(int Index)
        {
            _fileList.RemoveAt(Index);
        }

        public void MoveSong(int FromIndex, int ToIndex)
        {
            _fileList.Move(FromIndex, ToIndex);
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
            int Count = FileList.Count;
            int? WrappedIndex = (Index % Count + Count) % Count; // Keep index in range [0, Count[.
            ActiveSongIndex = WrappedIndex;
        }

        private void FileList_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            int FoundIndex = FileList.ToList().FindIndex(x => ReferenceEquals(x, ActiveSong));
            ActiveSongIndex = FoundIndex > -1 ? FoundIndex : null;
        }
    }
}
