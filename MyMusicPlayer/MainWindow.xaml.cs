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
        private Properties _properties;

        public MainWindow()
        {
            InitializeComponent();
            _properties = new Properties();
            DataContext = _properties;
        }

        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var OpenFileDialog = new CommonOpenFileDialog();
            OpenFileDialog.IsFolderPicker = true;
            if (OpenFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                _properties.CreateFileHierarchy(OpenFileDialog.FileName);
            }
        }
    }

    public class Properties : INotifyPropertyChanged
    {
        private FileHierarchyViewModel? FileHierarchyViewModel;
        public DirectoryViewModel? RootDirectoryViewModel { get => FileHierarchyViewModel?.RootViewModel; }
        public string? RootPath { get => RootDirectoryViewModel?.FullName; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public Properties()
        {
        }

        public void CreateFileHierarchy(string RootPath)
        {
            FileHierarchyViewModel = new FileHierarchyViewModel(RootPath);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RootDirectoryViewModel)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RootPath)));
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