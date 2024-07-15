using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimpleMusicPlayer.View
{
    public partial class DirectoryTreeView : UserControl
    {
        public static RoutedUICommand Command_PlayAllFiles = new(nameof(Command_PlayAllFiles), nameof(Command_PlayAllFiles), typeof(DirectoryTreeView));

        public event EventHandler PlayAllFiles;

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(DirectoryTreeView), new PropertyMetadata(null));

        public DirectoryTreeView()
        {
            InitializeComponent();
        }

        private void OpenFolderButton_Click(object Sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModel.DirectoryTree DirectoryTree)
            {
                var OpenFileDialog = new CommonOpenFileDialog();
                OpenFileDialog.IsFolderPicker = true;
                if (OpenFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    DirectoryTree.Clear();
                    DirectoryTree.AddDirectory(new System.IO.DirectoryInfo(OpenFileDialog.FileName));
                }
            }
        }

        private void PlayAllFiles_Executed(object Sender, ExecutedRoutedEventArgs e)
        {
            PlayAllFiles?.Invoke(Sender, EventArgs.Empty);
        }

        private void PlayAllFiles_CanExecute(object Sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
    }
}
