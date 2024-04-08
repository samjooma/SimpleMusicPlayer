using MyMusicPlayer;
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
            var FilesViewModel = new MyMusicPlayer.ViewModel.DirectoryHierarchy();
            FilesViewModel.SetRootDirectory(RootDirectoryName);
            Assert.AreEqual(RootDirectoryName, FilesViewModel.RootDirectory.Name, false);
        }

        [TestMethod]
        public void CreateViewModel_ChildrenAreNotNull()
        {
            var FilesViewModel = new MyMusicPlayer.ViewModel.DirectoryHierarchy();
            FilesViewModel.SetRootDirectory(RootDirectoryName);
            Assert.IsNotNull(FilesViewModel.RootDirectory.Children);
        }

        [TestMethod]
        public void ShrinkAndExpandRoot_RootContainsTwoChildren()
        {
            var FilesViewModel = new MyMusicPlayer.ViewModel.DirectoryHierarchy();
            FilesViewModel.SetRootDirectory(RootDirectoryName);
            FilesViewModel.RootDirectory.IsExpanded = false;
            FilesViewModel.RootDirectory.IsExpanded = true;
            Assert.AreEqual(2, FilesViewModel.RootDirectory.Children.Count());
        }

        [TestMethod]
        public void ExpandAndShrinkRoot_RootContainsTwoChildren()
        {
            var FilesViewModel = new MyMusicPlayer.ViewModel.DirectoryHierarchy();
            FilesViewModel.SetRootDirectory(RootDirectoryName);
            FilesViewModel.RootDirectory.IsExpanded = true;
            FilesViewModel.RootDirectory.IsExpanded = false;
            Assert.AreEqual(2, FilesViewModel.RootDirectory.Children.Count());
        }

        [TestMethod]
        public void ExpandAll_ChildrenAreNotNull()
        {
            var FilesViewModel = new MyMusicPlayer.ViewModel.DirectoryHierarchy();
            FilesViewModel.SetRootDirectory(RootDirectoryName);
            void ExpandAndAssert(MyMusicPlayer.ViewModel.Directory Directory)
            {
                Directory.IsExpanded = true;
                Assert.IsNotNull(Directory.Children);
                foreach (var Child in Directory.Children)
                {
                    ExpandAndAssert(Child);
                }
            }
            ExpandAndAssert(FilesViewModel.RootDirectory);
        }

        [TestMethod]
        public void ExpandAllAndShrinkRoot_RootContainsTwoChildren()
        {
            var FilesViewModel = new MyMusicPlayer.ViewModel.DirectoryHierarchy();
            FilesViewModel.SetRootDirectory(RootDirectoryName);
            void Expand(MyMusicPlayer.ViewModel.Directory Directory)
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
            Expand(FilesViewModel.RootDirectory);
            FilesViewModel.RootDirectory.IsExpanded = false;
            Assert.AreEqual(2, FilesViewModel.RootDirectory.Children.Count());
        }

        [TestMethod]
        public void ExpandAllAndShrinkRoot_TotalDirectoryCountIs22()
        {
            var FilesViewModel = new MyMusicPlayer.ViewModel.DirectoryHierarchy();
            FilesViewModel.SetRootDirectory(RootDirectoryName);
            void Expand(MyMusicPlayer.ViewModel.Directory Directory)
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
            Expand(FilesViewModel.RootDirectory);
            FilesViewModel.RootDirectory.IsExpanded = false;
            Assert.AreEqual(22, FilesViewModel.GetAllDirectories().Length);
        }

        [TestMethod]
        public void TrySelectingAllDirectories_OnlyOneIsSelected()
        {
            var FilesViewModel = new MyMusicPlayer.ViewModel.DirectoryHierarchy();
            FilesViewModel.SetRootDirectory(RootDirectoryName);
            void OpenAndSelect(MyMusicPlayer.ViewModel.Directory Directory)
            {
                Directory.IsExpanded = true;
                Directory.IsSelected = true;
                foreach (var Child in Directory.Children)
                {
                    OpenAndSelect(Child);
                }
            }
            OpenAndSelect(FilesViewModel.RootDirectory);

            int TotalSelected = FilesViewModel.GetAllDirectories().Count(x => x.IsSelected);
            Assert.AreEqual(1, TotalSelected);
        }

        [TestMethod]
        public void TrySelectingAllDirectories_SelectedReferenceIsCorrect()
        {
            var FilesViewModel = new MyMusicPlayer.ViewModel.DirectoryHierarchy();
            FilesViewModel.SetRootDirectory(RootDirectoryName);

            MyMusicPlayer.ViewModel.Directory LastSelected;
            void OpenAndSelect(MyMusicPlayer.ViewModel.Directory Directory)
            {
                Directory.IsExpanded = true;
                Directory.IsSelected = true;
                LastSelected = Directory;
                foreach (var Child in Directory.Children)
                {
                    OpenAndSelect(Child);
                }
            }
            OpenAndSelect(FilesViewModel.RootDirectory);

            Assert.AreSame(LastSelected, FilesViewModel.SelectedDirectory);
        }
    }
}
