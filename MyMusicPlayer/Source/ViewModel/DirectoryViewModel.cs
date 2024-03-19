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
        public DirectoryViewModel RootViewModel { get => DirectoryViewModels[Files.RootDirectory]; }

        public FileHierarchyViewModel(string RootDirectoryPath)
        {
            DirectoryViewModels = new Dictionary<DirectoryInfo, DirectoryViewModel>(new FileHierarchy.DirectoryComparer());
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

        private void Files_AfterDirectoryOpened(object? sender, DirectoryOpenedEventArgs e)
        {
            // Viewmodels for subdirectories.
            foreach (DirectoryInfo Subdirectory in e.SubDirectories)
            {
                DirectoryViewModels.TryAdd(Subdirectory, new DirectoryViewModel(this, Subdirectory));
            }
            var Children = new ObservableCollection<DirectoryViewModel>(e.SubDirectories.Select(x => DirectoryViewModels[x]));

            // Viewmodel for directory.
            DirectoryViewModels.TryAdd(e.Directory, new DirectoryViewModel(this, e.Directory));
        }

        private void Files_AfterDirectoryClosed(object? sender, DirectoryClosedEventArgs e)
        {
        }
    }

    public class DirectoryViewModel : INotifyPropertyChanged
    {
        public DirectoryInfo Directory { get; private set; }
        private FileHierarchyViewModel OwnerHierarchy;
        private bool _isExpanded;

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
        
        protected virtual void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
