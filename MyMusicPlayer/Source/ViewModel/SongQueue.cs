using MyMusicPlayer.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;

namespace MyMusicPlayer.ViewModel
{
    public class SongQueue : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private ObservableCollection<SongQueueItem> _fileList;
        public ReadOnlyObservableCollection<SongQueueItem> FileList { get; private set; }

        private SongQueueItem? _activeSong;
        public SongQueueItem? ActiveSong { get { return _activeSong; } }

        private int? _activeSongIndex;
        public int? ActiveSongIndex {
            get { return _activeSongIndex; }
            set
            {
                if (value != _activeSongIndex)
                {
                    _activeSongIndex = value;

                    if (_activeSong != null) _activeSong.IsSongActive = false;
                    _activeSong = _activeSongIndex != null ? _fileList[_activeSongIndex.Value] : null;
                    if (_activeSong != null) _activeSong.IsSongActive = true;

                    NotifyPropertyChanged(nameof(ActiveSongIndex));
                    NotifyPropertyChanged(nameof(ActiveSong));
                }
            }
        }

        public SongQueue()
        {
            _fileList = new ObservableCollection<SongQueueItem>();
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
            if (Index <= ActiveSongIndex)
            {
                ActiveSongIndex++;
            }
        }

        public void RemoveSongAt(int Index)
        {
            _fileList.RemoveAt(Index);
            if (Index == ActiveSongIndex)
            {
                ActiveSongIndex = null;
            }
            else if (Index < ActiveSongIndex)
            {
                ActiveSongIndex--;
            }
        }

        public void SetActiveIndexWrapped(int Index)
        {
            int Count = _fileList.Count;
            int? WrappedIndex = (Index % Count + Count) % Count; // Keep index in range [0, Count[.
            ActiveSongIndex = WrappedIndex;
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
    }
}
