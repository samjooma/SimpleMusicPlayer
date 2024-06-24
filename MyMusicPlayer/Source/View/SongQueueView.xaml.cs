using System;
using System.Collections.Generic;
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

namespace MyMusicPlayer.View
{
    public partial class SongQueueView : UserControl
    {
        public static RoutedUICommand PlaySelectedSong = new(nameof(PlaySelectedSong), nameof(PlaySelectedSong), typeof(MainWindow));

        public SongQueueView()
        {
            InitializeComponent();
        }

        private void SongQueueItem_PreviewMouseLeftButtonDown(object Sender, MouseButtonEventArgs e)
        {
            if (Sender is ListViewItem Item)
            {
                DragDrop.DoDragDrop(Item, Item, DragDropEffects.Move);
            }
        }

        private void SongQueueItem_Drop(object Sender, DragEventArgs e)
        {
            if (
                DataContext is ViewModel.SongQueue SongQueue &&
                Sender is ListViewItem Item &&
                e.Data.GetDataPresent(typeof(ListViewItem))
            )
            {
                var DragSource = (ListViewItem)e.Data.GetData(typeof(ListViewItem));
                int SourceIndex = SongListView.Items.IndexOf(DragSource.Content);
                int TargetIndex = SongListView.Items.IndexOf(Item.Content);
                SongQueue.MoveSong(SourceIndex, TargetIndex);
            }
        }

        private void CommandPlaySelectedSong_Executed(object Sender, ExecutedRoutedEventArgs e)
        {
            if (DataContext is ViewModel.SongQueue SongQueue)
            {
                SongQueue.ActiveSongIndex = SongListView.SelectedIndex;
            }
        }

        private void CommandPlaySelectedSong_CanExecute(object Sender, CanExecuteRoutedEventArgs e)
        {
            int Index = SongListView.SelectedIndex;
            e.CanExecute = Index > -1 && Index < SongListView.Items.Count;
        }

        private void CommandDelete_Executed(object Sender, ExecutedRoutedEventArgs e)
        {
            if (DataContext is ViewModel.SongQueue SongQueue)
            {
                SongQueue.RemoveSongAt(SongListView.SelectedIndex);
            }
        }

        private void CommandDelete_CanExecute(object Sender, CanExecuteRoutedEventArgs e)
        {
            int Index = SongListView.SelectedIndex;
            e.CanExecute = Index > -1 && Index < SongListView.Items.Count;
        }
    }
}
