﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace MyMusicPlayer.View
{
    /// <summary>
    /// Interaction logic for FileListView.xaml
    /// </summary>
    public partial class FileListView : UserControl
    {
        public event EventHandler ItemMouseDoubleClick;

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

        private void ListViewItem_MouseDoubleClick(object Sender, MouseButtonEventArgs e)
        {
            ItemMouseDoubleClick?.Invoke(Sender, e);
        }
    }
}
