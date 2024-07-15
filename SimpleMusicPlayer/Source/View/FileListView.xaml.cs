using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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
    /// <summary>
    /// Interaction logic for FileListView.xaml
    /// </summary>
    public partial class FileListView : UserControl
    {
        public static RoutedUICommand Command_AddSongToQueue = new(nameof(Command_AddSongToQueue), nameof(Command_AddSongToQueue), typeof(FileListView));

        public event EventHandler AddSongToQueue;

        private IEnumerable<FileInfo> Files
        {
            get => 
                DataContext is ViewModel.FileContainerNode FileContainer ?
                FileContainer.Files :
                [];
        }

        public FileListView()
        {
            InitializeComponent();
        }

        private void Command_AddSongToQueue_Executed(object Sender, ExecutedRoutedEventArgs e)
        {
            AddSongToQueue?.Invoke(e.OriginalSource, EventArgs.Empty);
        }

        private void Command_AddSongToQueue_CanExecute(object Sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = e.OriginalSource is FrameworkElement Element && Element.DataContext is FileInfo;
        }
    }
}
