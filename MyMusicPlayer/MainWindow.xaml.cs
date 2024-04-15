using Microsoft.Win32;
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
        public ViewModel.FileSelectionList DirectoryFileList { get; private set; }
        public ViewModel.FileSelectionList CurrentlyPlayingFileList { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainWindow()
        {
            Player = new ViewModel.AudioPlayer();
            Player.PropertyChanged += Player_PropertyChanged;
            DirectoryFileList = new ViewModel.FileSelectionList(true);
            CurrentlyPlayingFileList = new ViewModel.FileSelectionList(false);
            InitializeComponent();
        }

        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var OpenFileDialog = new CommonOpenFileDialog();
            OpenFileDialog.IsFolderPicker = true;
            if (OpenFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                SetRootDirectory(OpenFileDialog.FileName);
            }
        }

        private void SetRootDirectory(string RootDirectoryPath)
        {
            Directories = new ViewModel.DirectoryTree(RootDirectoryPath);
            Directories.PropertyChanged += Directories_PropertyChanged;
            NotifyPropertyChanged(nameof(Directories));
        }

        private void Directories_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender == null) throw new ArgumentNullException();
            if (sender != Directories) throw new ArgumentException();
            if (e.PropertyName == nameof(Directories.SelectedItem))
            {
                if (Directories.SelectedItem != null)
                {
                    DirectoryFileList.SetFiles(Directories.SelectedItem.AudioFiles);
                }
            }
        }

        private void Player_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender == null) throw new ArgumentNullException();
            if (sender != Player) throw new ArgumentException();
        }

        protected virtual void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            Player.IsPaused = !Player.IsPaused;
        }

        private void DirectoryView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListView Item)
            {
                if (Item.SelectedItem is FileInfo File)
                {
                    CurrentlyPlayingFileList.AddFile(File);
                }
            }
        }

        private void CurrentlyPlayingView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            FileInfo? SelectedFile = CurrentlyPlaying.SelectedItem as FileInfo;
            if (SelectedFile != null)
            {
                Player.PlayFile(SelectedFile);
            }
        }

        private void CommandPlay_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            FileInfo? SelectedFile = CurrentlyPlaying.SelectedItem as FileInfo;
            if (SelectedFile != null)
            {
                Player.PlayFile(SelectedFile);
            }
        }

        private void CommandPlay_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            FileInfo? SelectedFile = CurrentlyPlaying.SelectedItem as FileInfo;
            e.CanExecute = SelectedFile != null;
        }

        private void CommandDelete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CurrentlyPlayingFileList.RemoveFiles(CurrentlyPlaying.SelectedItems.OfType<FileInfo>());
        }

        private void CommandDelete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
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