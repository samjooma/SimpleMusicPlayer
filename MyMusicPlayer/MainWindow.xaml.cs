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
using System.Windows.Shapes;
using System.Diagnostics;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.ObjectModel;
using MyMusicPlayer.ViewModel;
using System.Globalization;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;

namespace MyMusicPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FileHierarchyProperties _filesProperties;

        public MainWindow()
        {
            InitializeComponent();
            _filesProperties = new FileHierarchyProperties();
            DataContext = _filesProperties;
        }

        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var OpenFileDialog = new CommonOpenFileDialog();
            OpenFileDialog.IsFolderPicker = true;
            if (OpenFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                _filesProperties.SetRootDirectory(OpenFileDialog.FileName);
            }
        }
    }

    public class FileHierarchyProperties : INotifyPropertyChanged
    {
        private FileHierarchyViewModel? FileHierarchyViewModel;
        public DirectoryViewModel? RootDirectoryViewModel { get => FileHierarchyViewModel?.RootViewModel; }
        public string? RootPath { get => RootDirectoryViewModel?.FullName; }
        public string? SelectedDirectoryName { get => FileHierarchyViewModel?.SelectedDirectoryViewModel?.Name; }
        
        public event PropertyChangedEventHandler? PropertyChanged;

        public FileHierarchyProperties()
        {
        }

        public void SetRootDirectory(string RootDirectoryPath)
        {
            FileHierarchyViewModel = new FileHierarchyViewModel();
            FileHierarchyViewModel.SetRootDirectory(RootDirectoryPath);
            FileHierarchyViewModel.PropertyChanged += FileHierarchyViewModel_PropertyChanged;
            NotifyPropertyChanged(nameof(RootDirectoryViewModel));
            NotifyPropertyChanged(nameof(RootPath));
        }

        private void FileHierarchyViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FileHierarchyViewModel.SelectedDirectoryViewModel))
            {
                NotifyPropertyChanged(nameof(SelectedDirectoryName));
            }
        }

        protected virtual void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
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
}