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
    public class FileHierarchyViewModel : INotifyPropertyChanged
    {
        private FileHierarchy Files { get; set; }
        private Dictionary<DirectoryInfo, DirectoryViewModel> DirectoryViewModels { get; set; }
        private DirectoryViewModel? _selectedDirectoryViewModel;
        public DirectoryViewModel RootViewModel { get => DirectoryViewModels[Files.RootDirectory]; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public FileHierarchyViewModel()
        {
            DirectoryViewModels = new Dictionary<DirectoryInfo, DirectoryViewModel>(new FileHierarchy.DirectoryComparer());
            Files = new FileHierarchy();
        }

        public DirectoryViewModel? SelectedDirectoryViewModel
        {
            get => _selectedDirectoryViewModel;
            private set
            {
                if (value != _selectedDirectoryViewModel)
                {
                    _selectedDirectoryViewModel = value;
                    NotifyPropertyChanged(nameof(SelectedDirectoryViewModel));
                }
            }
        }

        public void SetRootDirectory(string RootDirectoryPath)
        {
            DirectoryViewModels.Clear();
            Files = new FileHierarchy();
            Files.AfterDirectoryOpened += Files_AfterDirectoryOpened;
            Files.AfterDirectoryClosed += Files_AfterDirectoryClosed;
            Files.OpenDirectory(RootDirectoryPath);
        }

        public DirectoryViewModel[] GetAllDirectoryViewModels()
        {
            return DirectoryViewModels.Values.ToArray();
        }

        public IEnumerable<DirectoryViewModel> GetChildren(DirectoryViewModel Directory)
        {
            return Files.GetSubDirectories(Directory.Directory).Select(x => DirectoryViewModels[x]);
        }

        public void OpenDirectory(DirectoryInfo Directory)
        {
            Files.OpenDirectory(Directory);
        }

        private void TryAddDirectoryViewModel(DirectoryInfo Key, DirectoryViewModel Value)
        {
            if (DirectoryViewModels.TryAdd(Key, Value))
            {
                Value.PropertyChanged += DirectoryViewModel_PropertyChanged;
            }
        }

        private void DirectoryViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is DirectoryViewModel ChangedDirectory)
            {
                if (e.PropertyName == nameof(ChangedDirectory.IsSelected))
                {
                    if (ChangedDirectory.IsSelected)
                    {
                        // Deselect currently selected directory.
                        if (SelectedDirectoryViewModel != null)
                        {
                            SelectedDirectoryViewModel.IsSelected = false;
                        }
                        // Set reference to new selected object.
                        SelectedDirectoryViewModel = ChangedDirectory;
                    }
                    else
                    {
                        SelectedDirectoryViewModel = null;
                    }
                }
            }
        }

        private void Files_AfterDirectoryOpened(object? sender, DirectoryOpenedEventArgs e)
        {
            // Viewmodels for subdirectories.
            foreach (DirectoryInfo Subdirectory in e.SubDirectories)
            {
                TryAddDirectoryViewModel(Subdirectory, new DirectoryViewModel(this, Subdirectory));
            }
            var Children = new ObservableCollection<DirectoryViewModel>(e.SubDirectories.Select(x => DirectoryViewModels[x]));

            // Viewmodel for directory.
            TryAddDirectoryViewModel(e.Directory, new DirectoryViewModel(this, e.Directory));
        }

        private void Files_AfterDirectoryClosed(object? sender, DirectoryClosedEventArgs e)
        {
        }

        protected virtual void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }

    public class DirectoryViewModel : INotifyPropertyChanged
    {
        public DirectoryInfo Directory { get; private set; }
        private FileHierarchyViewModel OwnerHierarchy;
        private bool _isExpanded;
        private bool _isSelected;

        public IEnumerable<DirectoryViewModel> Children { get => OwnerHierarchy.GetChildren(this); }
        public string Name { get => Directory.Name; }
        public string FullName { get => Directory.FullName; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public DirectoryViewModel(FileHierarchyViewModel Owner, DirectoryInfo Directory)
        {
            OwnerHierarchy = Owner;
            this.Directory = Directory;
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

                    if (_isExpanded)
                    {
                        foreach (DirectoryViewModel Child in Children)
                        {
                            OwnerHierarchy.OpenDirectory(Child.Directory);
                        }
                    }
                }
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    NotifyPropertyChanged(nameof(IsSelected));
                }
            }
        }

        protected virtual void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
