using Microsoft.VisualBasic;
using MyMusicPlayer.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using MS.WindowsAPICodePack.Internal;
using System.Diagnostics;

namespace MyMusicPlayer.ViewModel
{
    public class FileHierarchyViewModel
    {
        private FileHierarchy Files { get; set; }
        private Dictionary<DirectoryInfo, DirectoryViewModel> DirectoryViewModels { get; set; }
        public DirectoryViewModel RootViewModel {
            get => DirectoryViewModels[Files.RootDirectory];
        }

        public FileHierarchyViewModel(string RootDirectoryPath)
        {
            DirectoryViewModels = new Dictionary<DirectoryInfo, DirectoryViewModel>();
            Files = new FileHierarchy(RootDirectoryPath);
            Files.AfterDirectoryOpened += Files_AfterDirectoryOpened;
            Files.AfterDirectoryClosed += Files_AfterDirectoryClosed;
        }

        private bool TryAddDirectoryViewModel(DirectoryInfo Key, DirectoryViewModel Value)
        {
            if (DirectoryViewModels.TryAdd(Key, Value))
            {
                Value.PropertyChanged += DirectoryViewModel_PropertyChanged;
                return true;
            }
            return false;
        }

        private void Files_AfterDirectoryOpened(object? sender, DirectoryOpenedEventArgs e)
        {
            // Create viewmodels for the opened directory and its subdirectories.

            // Viewmodels for subdirectories.
            foreach (DirectoryInfo SubDirectory in e.SubDirectories)
            {
                TryAddDirectoryViewModel(SubDirectory, new DirectoryViewModel(SubDirectory, null));
            }
            var Children = new ObservableCollection<DirectoryViewModel>(e.SubDirectories.Select(x => DirectoryViewModels[x]));

            // Viewmodel for directory.
            if (!TryAddDirectoryViewModel(e.Directory, new DirectoryViewModel(e.Directory, Children)))
            {
                // Viewmodel already exists, just update its children.
                DirectoryViewModels[e.Directory].Children = Children;
            }
        }

        private void Files_AfterDirectoryClosed(object? sender, DirectoryClosedEventArgs e)
        {
            DirectoryViewModels.Remove(e.Directory);
        }

        private void DirectoryViewModel_PropertyChanged(object? Sender, PropertyChangedEventArgs e)
        {
            if (Sender is DirectoryViewModel DirectoryViewModel && e.PropertyName == nameof(DirectoryViewModel.IsExpanded))
            {
                if (DirectoryViewModel.Children == null)
                {
                    throw new ArgumentNullException("DirectoryViewModel.Children == null"); //TODO: Better exception.
                }
                if (DirectoryViewModel.IsExpanded)
                {
                    foreach (DirectoryViewModel Child in DirectoryViewModel.Children)
                    {
                        Files.OpenDirectory(Child.Directory);
                    }
                }
                else
                {
                    foreach (DirectoryViewModel Child in DirectoryViewModel.Children)
                    {
                        Files.CloseDirectory(Child.Directory);
                    }
                }
            }
        }
    }

    public class DirectoryViewModel : INotifyPropertyChanged
    {
        public DirectoryInfo Directory { get; private set; }
        public ObservableCollection<DirectoryViewModel>? Children { get; set; }

        public string Name { get => Directory.Name; }
        public string FullName { get => Directory.FullName; }
        private bool _isExpanded;

        public event PropertyChangedEventHandler? PropertyChanged;

        public DirectoryViewModel(DirectoryInfo Directory, ObservableCollection<DirectoryViewModel>? Children)
        {
            this.Directory = Directory;
            this.Children = Children;
            _isExpanded = false;
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    NotifyPropertyChanged(nameof(IsExpanded));
                }
            }
        }
        
        protected virtual void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
