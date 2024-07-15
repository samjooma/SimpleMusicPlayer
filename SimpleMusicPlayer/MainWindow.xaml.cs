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
using SimpleMusicPlayer.View;

namespace SimpleMusicPlayer
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

        private void FileListView_AddSongToQueue(object Sender, EventArgs e)
        {
            if (Sender is FrameworkElement Element && Element.DataContext is System.IO.FileInfo File)
            {
                SongQueue.AddSong(File);
            }
        }

        private void DirectoryTreeView_PlayAllFiles(object Sender, EventArgs e)
        {
            if (Sender is TreeViewItem Item && Item.DataContext is ViewModel.FileContainerNode Container)
            {
                SongQueue.Clear();
                foreach (var File in Container.Files)
                {
                    SongQueue.AddSong(File);
                }
                if (SongQueue.SongQueueItems.Count > 0)
                {
                    SongQueue.ActiveSongIndex = 0;
                    Player.Play();
                }
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
            e.CanExecute = SongQueue.SongQueueItems.Count > 0;
        }

        private void CommandStop_Executed(object Sender, ExecutedRoutedEventArgs e)
        {
            Player.Stop();
        }

        private void CommandStop_CanExecute(object Sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SongQueue.SongQueueItems.Count > 0;
        }

        private void CommandPreviousTrack_Executed(object Sender, ExecutedRoutedEventArgs e)
        {
            SongQueue.PreviousSong();
        }

        private void CommandPreviousTrack_CanExecute(object Sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SongQueue.SongQueueItems.Count > 0;
        }

        private void CommandNextTrack_Executed(object Sender, ExecutedRoutedEventArgs e)
        {
            SongQueue.NextSong();
        }

        private void CommandNextTrack_CanExecute(object Sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SongQueue.SongQueueItems.Count > 0;
        }
    }
}