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

namespace MyMusicPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public ViewModel.DirectoryHierarchy? Directories { get; private set; }
        public ViewModel.SoundPlayer Player { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainWindow()
        {
            Player = new ViewModel.SoundPlayer();
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

        public void SetRootDirectory(string RootDirectoryPath)
        {
            Directories = new ViewModel.DirectoryHierarchy();
            Directories.SetRootDirectory(RootDirectoryPath);
            NotifyPropertyChanged(nameof(Directories));
        }

        protected virtual void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            Player.TogglePause();
        }

        private void ListView_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListView Item)
            {
                if (Item.SelectedItem is FileInfo File)
                {
                    Player.PlayFile(File);
                }
            }
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