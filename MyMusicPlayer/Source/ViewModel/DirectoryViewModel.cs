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
        private FileHierarchy Files;
        public DirectoryViewModel RootViewModel { get; private set; }

        public FileHierarchyViewModel(string RootDirectoryPath)
        {
            Files = new FileHierarchy(RootDirectoryPath);
            Files.UpdateSubDirectories(Files.RootDirectory);
            RootViewModel = new DirectoryViewModel(Files, Files.RootDirectory, null);
        } 
    }

    public class DirectoryViewModel : INotifyPropertyChanged
    {
        private readonly FileHierarchy Files;
        private readonly DirectoryInfo Directory;

        public DirectoryViewModel? Parent { get; private set; }
        public ObservableCollection<DirectoryViewModel>? Children { get; private set; }

        public string Name { get => Directory.Name; }
        public string FullName { get => Directory.FullName; }
        private bool _isExpanded;

        public DirectoryViewModel(FileHierarchy Files, DirectoryInfo Directory, DirectoryViewModel? Parent)
        {
            this.Files = Files;
            this.Directory = Directory;
            this.Parent = Parent;
            UpdateChildren(); // Root directory's children should be filled immediately.

            _isExpanded = false;
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (value == _isExpanded) return;

                _isExpanded = value;
                NotifyPropertyChanged(nameof(IsExpanded));

                if (Children != null)
                {
                    foreach (DirectoryViewModel Child in Children)
                    {
                        if (_isExpanded)
                        {
                            Files.UpdateSubDirectories(Child.Directory);
                        }
                        else
                        {
                            Files.ClearSubDirectories(Child.Directory);
                        }
                        Child.UpdateChildren();
                    }
                }
            }
        }

        private void UpdateChildren()
        {
            DirectoryInfo[]? SubDirectories = Files.GetSubDirectories(Directory);
            if (SubDirectories == null)
            {
                Children = null;
            }
            else
            {
                Children = new ObservableCollection<DirectoryViewModel>();
                foreach (DirectoryInfo SubDirectory in SubDirectories)
                {
                    Children.Add(new DirectoryViewModel(Files, SubDirectory, this));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
