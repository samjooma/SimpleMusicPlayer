﻿using Microsoft.Win32;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Diagnostics;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.ObjectModel;
using System.Globalization;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;
using System.IO;
using System.Windows.Controls.Primitives;
using System.Collections.Specialized;
using System.Data;

namespace MyMusicPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public ViewModel.DirectoryTree? Directories { get; private set; }
        public ViewModel.AudioPlayer Player { get; private set; }
        public ViewModel.SongQueue SongQueue { get; private set; }
        public ObservableCollection<ViewModel.FileItem> DirectoryFiles { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainWindow()
        {
            Player = new ViewModel.AudioPlayer();
            Player.PropertyChanged += Player_PropertyChanged;
            SongQueue = new ViewModel.SongQueue();
            SongQueue.PropertyChanged += SongQueue_PropertyChanged;
            DirectoryFiles = new ObservableCollection<ViewModel.FileItem>();
            InitializeComponent();
        }

        private void SetRootDirectory(string RootDirectoryPath)
        {
            Directories = new ViewModel.DirectoryTree(RootDirectoryPath);
            Directories.PropertyChanged += Directories_PropertyChanged;
            NotifyPropertyChanged(nameof(Directories));
        }

        private void PlaySongInQueueAtIndex(int Index)
        {
            var SongItems = SongQueueView.Items.OfType<ViewModel.SongQueueItem>();
            Player.PlayFile(SongItems.ElementAt(Index).FileInfo);
            SongQueue.ActiveSongIndex = Index;
        }

        //
        // User interface events.
        //

        private void OpenFolderButton_Click(object Sender, RoutedEventArgs e)
        {
            var OpenFileDialog = new CommonOpenFileDialog();
            OpenFileDialog.IsFolderPicker = true;
            if (OpenFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                SetRootDirectory(OpenFileDialog.FileName);
            }
        }

        private void PauseButton_Click(object Sender, RoutedEventArgs e)
        {
            Player.IsPaused = !Player.IsPaused;
        }

        private void DirectoryView_MouseDoubleClick(object Sender, MouseButtonEventArgs e)
        {
            if (Sender is ListView View)
            {
                if (View.SelectedItem is ViewModel.FileItem Item)
                {
                    SongQueue.AddSong(new ViewModel.SongQueueItem(Item.FileInfo, false));
                }
            }
        }

        private void SongQueueView_MouseDoubleClick(object Sender, MouseButtonEventArgs e)
        {
            if (SongQueueView.SelectedIndex > -1)
            {
                PlaySongInQueueAtIndex(SongQueueView.SelectedIndex);
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
            if (Sender != Directories) throw new ArgumentException();
            if (Directories == null) throw new ArgumentNullException();
            if (e.PropertyName == nameof(Directories.SelectedItem))
            {
                if (Directories.SelectedItem != null)
                {
                    DirectoryFiles.Clear();
                    foreach (var File in Directories.SelectedItem.AudioFiles)
                    {
                        var Item = new ViewModel.FileItem(File);
                        DirectoryFiles.Add(Item);
                    }
                }
            }
        }

        private void Player_PropertyChanged(object? Sender, PropertyChangedEventArgs e)
        {
        }

        private void SongQueue_PropertyChanged(object? Sender, PropertyChangedEventArgs e)
        {
        }

        //
        // Commands.
        //

        private void CommandPlay_Executed(object Sender, ExecutedRoutedEventArgs e)
        {
            if (SongQueueView.SelectedIndex > -1)
            {
                PlaySongInQueueAtIndex(SongQueueView.SelectedIndex);
            }
        }

        private void CommandPlay_CanExecute(object Sender, CanExecuteRoutedEventArgs e)
        {
            int Index = SongQueueView.SelectedIndex;
            e.CanExecute = Index > -1 && Index < SongQueueView.Items.Count;
        }

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

        private void CommandPreviousTrack_Executed(object Sender, ExecutedRoutedEventArgs e)
        {
            
        }

        private void CommandPreviousTrack_CanExecute(object Sender, CanExecuteRoutedEventArgs e)
        {

        }
    }

    public class ObjectArrayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new object[] { value };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FileNameWithoutExtension : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Path.GetFileNameWithoutExtension((value as string) ?? "");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanConverter : IValueConverter
    {
        public string FalseValue { get; set; }
        public string TrueValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return FalseValue!;
            return (bool)value ? TrueValue! : FalseValue!;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return false;
            return value.Equals(TrueValue);
        }
    }
}