using MyMusicPlayer.Model;
using MyMusicPlayer.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestProject
{
    [TestClass]
    [DeploymentItem("Test files", "Test files")]
    public class DirectoryViewModelTests
    {
        private const string RootDirectoryName = "Test files";

        [TestMethod]
        public void CreateViewModel_RootNameIsValid()
        {
            var FilesViewModel = new FileHierarchyViewModel();
            FilesViewModel.SetRootDirectory(RootDirectoryName);
            Assert.AreEqual(RootDirectoryName, FilesViewModel.RootViewModel.Name, false);
        }

        [TestMethod]
        public void CreateViewModel_ChildrenAreNotNull()
        {
            var FilesViewModel = new FileHierarchyViewModel();
            FilesViewModel.SetRootDirectory(RootDirectoryName);
            Assert.IsNotNull(FilesViewModel.RootViewModel.Children);
        }

        [TestMethod]
        public void ShrinkAndExpandRoot_RootContainsTwoChildren()
        {
            var FilesViewModel = new FileHierarchyViewModel();
            FilesViewModel.SetRootDirectory(RootDirectoryName);
            FilesViewModel.RootViewModel.IsExpanded = false;
            FilesViewModel.RootViewModel.IsExpanded = true;
            Assert.AreEqual(2, FilesViewModel.RootViewModel.Children.Count());
        }

        [TestMethod]
        public void ExpandAndShrinkRoot_RootContainsTwoChildren()
        {
            var FilesViewModel = new FileHierarchyViewModel();
            FilesViewModel.SetRootDirectory(RootDirectoryName);
            FilesViewModel.RootViewModel.IsExpanded = true;
            FilesViewModel.RootViewModel.IsExpanded = false;
            Assert.AreEqual(2, FilesViewModel.RootViewModel.Children.Count());
        }

        [TestMethod]
        public void ExpandAll_ChildrenAreNotNull()
        {
            var FilesViewModel = new FileHierarchyViewModel();
            FilesViewModel.SetRootDirectory(RootDirectoryName);
            void ExpandAndAssert(DirectoryViewModel Directory)
            {
                Directory.IsExpanded = true;
                Assert.IsNotNull(Directory.Children);
                foreach (var Child in Directory.Children)
                {
                    ExpandAndAssert(Child);
                }
            }
            ExpandAndAssert(FilesViewModel.RootViewModel);
        }

        [TestMethod]
        public void ExpandAllAndShrinkRoot_RootContainsTwoChildren()
        {
            var FilesViewModel = new FileHierarchyViewModel();
            FilesViewModel.SetRootDirectory(RootDirectoryName);
            void Expand(DirectoryViewModel Directory)
            {
                Directory.IsExpanded = true;
                if (Directory.Children != null)
                {
                    foreach (var Child in Directory.Children)
                    {
                        Expand(Child);
                    }
                }
            }
            Expand(FilesViewModel.RootViewModel);
            FilesViewModel.RootViewModel.IsExpanded = false;
            Assert.AreEqual(2, FilesViewModel.RootViewModel.Children.Count());
        }

        [TestMethod]
        public void ExpandAllAndShrinkRoot_TotalDirectoryCountIs22()
        {
            var FilesViewModel = new FileHierarchyViewModel();
            FilesViewModel.SetRootDirectory(RootDirectoryName);
            void Expand(DirectoryViewModel Directory)
            {
                Directory.IsExpanded = true;
                if (Directory.Children != null)
                {
                    foreach (var Child in Directory.Children)
                    {
                        Expand(Child);
                    }
                }
            }
            Expand(FilesViewModel.RootViewModel);
            FilesViewModel.RootViewModel.IsExpanded = false;
            Assert.AreEqual(22, FilesViewModel.GetAllDirectoryViewModels().Length);
        }

        [TestMethod]
        public void TrySelectingAllDirectories_OnlyOneIsSelected()
        {
            var FilesViewModel = new FileHierarchyViewModel();
            FilesViewModel.SetRootDirectory(RootDirectoryName);
            void OpenAndSelect(DirectoryViewModel Directory)
            {
                FilesViewModel.OpenDirectory(Directory.Directory);
                Directory.IsSelected = true;
                if (Directory.Children != null)
                {
                    foreach (var Child in Directory.Children)
                    {
                        OpenAndSelect(Child);
                    }
                }
            }
            OpenAndSelect(FilesViewModel.RootViewModel);

            int TotalSelected = FilesViewModel.GetAllDirectoryViewModels().Count(x => x.IsSelected);
            Assert.AreEqual(1, TotalSelected);
        }

        [TestMethod]
        public void TrySelectingAllDirectories_SelectedReferenceIsCorrect()
        {
            var FilesViewModel = new FileHierarchyViewModel();
            FilesViewModel.SetRootDirectory(RootDirectoryName);

            DirectoryViewModel LastSelected;
            void OpenAndSelect(DirectoryViewModel Directory)
            {
                FilesViewModel.OpenDirectory(Directory.Directory);
                Directory.IsSelected = true;
                LastSelected = Directory;
                if (Directory.Children != null)
                {
                    foreach (var Child in Directory.Children)
                    {
                        OpenAndSelect(Child);
                    }
                }
            }
            OpenAndSelect(FilesViewModel.RootViewModel);

            Assert.AreSame(LastSelected, FilesViewModel.SelectedDirectoryViewModel);
        }
    }
}
