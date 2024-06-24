using Microsoft.Win32;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.ObjectModel;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;
using System.Windows.Controls.Primitives;
using System.Collections.Specialized;

namespace MyMusicPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public ViewModel.DirectoryTree DirectoryTree { get; private set; }
        public ViewModel.AudioPlayer Player { get; private set; }
        public ViewModel.SongQueue SongQueue { get; private set; }
        public ObservableCollection<ViewModel.FileItem> FilesInSelectedDirectory { get; private set; }

        public static RoutedUICommand PlaySelectedSong = new(nameof(PlaySelectedSong), nameof(PlaySelectedSong), typeof(MainWindow));
        public static RoutedUICommand AddSongToQueue = new(nameof(AddSongToQueue), nameof(AddSongToQueue), typeof(MainWindow));

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainWindow()
        {
            DirectoryTree = new ViewModel.DirectoryTree();
            DirectoryTree.PropertyChanged += Directories_PropertyChanged;
            Player = new ViewModel.AudioPlayer();
            SongQueue = new ViewModel.SongQueue();
            SongQueue.PropertyChanged += SongQueue_PropertyChanged;
            FilesInSelectedDirectory = new ObservableCollection<ViewModel.FileItem>();
            InitializeComponent();
        }

        //
        // User interface events.
        //

        private void SongQueueItem_PreviewMouseLeftButtonDown(object Sender, MouseButtonEventArgs e)
        {
            if (Sender is ListViewItem Item)
            {
                DragDrop.DoDragDrop(Item, Item, DragDropEffects.Move);
            }
        }

        private void SongQueueItem_Drop(object Sender, DragEventArgs e)
        {
            if (Sender is ListViewItem Item)
            {
                if (e.Data.GetDataPresent(typeof(ListViewItem)))
                {
                    var DragSource = (ListViewItem)e.Data.GetData(typeof(ListViewItem));
                    int SourceIndex = SongQueueView.Items.IndexOf(DragSource.Content);
                    int TargetIndex = SongQueueView.Items.IndexOf(Item.Content);
                    SongQueue.MoveSong(SourceIndex, TargetIndex);
                }
            }
        }

        private void FileListView_ItemMouseDoubleClick(object Sender, EventArgs e)
        {
            if (Sender is ListViewItem ViewItem && ViewItem.Content is System.IO.FileInfo File)
            {
                SongQueue.AddSong(new ViewModel.SongQueueItem(File, false));
            }
        }

        //
        // Property changed events.
        //

        protected virtual void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        private void Directories_PropertyChanged(object? Sender, PropertyChangedEventArgs e)
        {
            if (Sender != DirectoryTree) throw new ArgumentException();
            if (e.PropertyName == nameof(DirectoryTree.SelectedNode))
            {
                if (DirectoryTree.SelectedNode != null)
                {
                    FilesInSelectedDirectory.Clear();
                    foreach (var File in DirectoryTree.SelectedNode.Files)
                    {
                        var Item = new ViewModel.FileItem(File);
                        FilesInSelectedDirectory.Add(Item);
                    }
                }
            }
        }

        private void SongQueue_PropertyChanged(object? Sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SongQueue.ActiveSong))
            {
                if (SongQueue.ActiveSong == null)
                {
                    Player.CloseFile();
                }
                else
                {
                    Player.OpenFile(SongQueue.ActiveSong.FileInfo);
                }
            }
        }

        //
        // Commands.
        //

        private void CommandDelete_Executed(object Sender, ExecutedRoutedEventArgs e)
        {
            if (SongQueueView.SelectedIndex > -1)
            {
                SongQueue.RemoveSongAt(SongQueueView.SelectedIndex);
            }
        }

        private void CommandDelete_CanExecute(object Sender, CanExecuteRoutedEventArgs e)
        {
            int Index = SongQueueView.SelectedIndex;
            e.CanExecute = Index > -1 && Index < SongQueueView.Items.Count;
        }

        private void CommandPlay_Executed(object Sender, ExecutedRoutedEventArgs e)
        {
        }

        private void CommandPlay_CanExecute(object Sender, CanExecuteRoutedEventArgs e)
        {
        }

        private void CommandTogglePlayPause_Executed(object Sender, ExecutedRoutedEventArgs e)
        {
            Player.TogglePause();
        }

        private void CommandTogglePlayPause_CanExecute(object Sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SongQueue.FileList.Count > 0;
        }

        private void CommandStop_Executed(object Sender, ExecutedRoutedEventArgs e)
        {
            Player.Stop();
        }

        private void CommandStop_CanExecute(object Sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SongQueue.FileList.Count > 0;
        }

        private void CommandPreviousTrack_Executed(object Sender, ExecutedRoutedEventArgs e)
        {
            SongQueue.PreviousSong();
        }

        private void CommandPreviousTrack_CanExecute(object Sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SongQueue.FileList.Count > 0;
        }

        private void CommandNextTrack_Executed(object Sender, ExecutedRoutedEventArgs e)
        {
            SongQueue.NextSong();
        }

        private void CommandNextTrack_CanExecute(object Sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SongQueue.FileList.Count > 0;
        }

        private void CommandPlaySelectedSong_Executed(object Sender, ExecutedRoutedEventArgs e)
        {
            SongQueue.ActiveSongIndex = SongQueueView.SelectedIndex;
        }

        private void CommandPlaySelectedSong_CanExecute(object Sender, CanExecuteRoutedEventArgs e)
        {
            int Index = SongQueueView.SelectedIndex;
            e.CanExecute = Index > -1 && Index < SongQueueView.Items.Count;
        }
    }
}