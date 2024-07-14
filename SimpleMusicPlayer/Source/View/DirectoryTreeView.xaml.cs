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
    /// <summary>
    /// Interaction logic for DirectoryTreeView.xaml
    /// </summary>
    public partial class DirectoryTreeView : UserControl
    {
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
    }
}
