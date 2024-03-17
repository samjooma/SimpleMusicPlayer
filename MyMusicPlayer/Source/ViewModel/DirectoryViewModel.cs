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
            Files = new FileHierarchy(RootDirectoryPath);
            DirectoryViewModels = new Dictionary<DirectoryInfo, DirectoryViewModel>();
            OpenDirectory(Files.RootDirectory);
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

        private void OpenDirectory(DirectoryInfo Directory)
        {
            // Add subdirectories.
            DirectoryInfo[] SubDirectories = Files.OpenDirectory(Directory);
            foreach (DirectoryInfo SubDirectory in SubDirectories)
            {
                TryAddDirectoryViewModel(SubDirectory, new DirectoryViewModel(SubDirectory, null));
            }
            var Children = new ObservableCollection<DirectoryViewModel>(SubDirectories.Select(x => DirectoryViewModels[x]));

            // Add directory.
            if (!TryAddDirectoryViewModel(Directory, new DirectoryViewModel(Directory, Children)))
            {
                // Viewmodel already exists, just update its children.
                DirectoryViewModels[Directory].Children = Children;
            }
        }

        private void CloseDirectory(DirectoryInfo Directory)
        {
            Files.CloseDirectory(Directory);
            DirectoryViewModels.Remove(Directory);
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
                        OpenDirectory(Child.Directory);
                    }
                }
                else
                {
                    foreach (DirectoryViewModel Child in DirectoryViewModel.Children)
                    {
                        CloseDirectory(Child.Directory);
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
