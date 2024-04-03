using MyMusicPlayer.Model;
using MyMusicPlayer.ViewModel;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;

namespace UnitTestProject
{
    [TestClass]
    [DeploymentItem("Test files", "Test files")]
    public class FileHierarchyTests
    {
        private const string RootDirectoryName = "Test files";

        /// <summary>
        /// Used for testing file hierarchy trees.
        /// </summary>
        /// <param name="Name">Name of a file or directory</param>
        /// <param name="Children">Subdirectories of this directory</param>
        public class FileNameTree
        {
            public string Name;
            public FileNameTree[] Children;
            public FileNameTree(string Name, IEnumerable<FileNameTree> Children)
            {
                this.Name = Name;
                this.Children = Children.ToArray();
            }
            public FileNameTree(string Name) : this(Name, Array.Empty<FileNameTree>()) {}
        }
        public static IEnumerable<object[]> AllDirectoriesNameTree // TODO: Make this a JSON or XML file.
        {
            get
            {
                return new[]
                {
                    new[]
                    {
                        new FileNameTree(RootDirectoryName, [
                            new ("Kevin MacLeod", [
                                new ("Ambient"),
                                new ("Electronic"),
                                new ("Other")
                            ]),
                            new ("Public domain", [
                                new ("1920s", [
                                    new ("a", [
                                        new ("b", [
                                            new ("c"),
                                            new ("d"),
                                            new ("e")
                                        ])
                                    ]),
                                    new ("stuff"),
                                ]),
                                new ("1926", [
                                    new ("Virginia Liston")
                                ]),
                                new ("1927", [
                                    new ("Blind Lemon Jefferson"),
                                    new ("Burnett And Rutherford"),
                                    new ("Dock Boggs"),
                                    new ("G B Grayson"),
                                    new ("Kelly Harrell", [
                                        new ("Kelly Harrell And The Virginia String Band")
                                    ])
                                ])
                            ])
                        ])
                    }
                };
            }
        }

        [TestMethod]
        public void CreateRoot_NameIsValid()
        {
            var Files = new DirectoryHierarchy();
            Files.OpenDirectory(RootDirectoryName);
            Assert.AreEqual(RootDirectoryName, Files.RootDirectory.Name, false);
        }

        [TestMethod]
        public void OpenRootSubDirectories_ContainsTwo()
        {
            var Files = new DirectoryHierarchy();
            Files.OpenDirectory(RootDirectoryName);
            var SubDirectories = Files.OpenDirectory(Files.RootDirectory);
            Assert.AreEqual(2, SubDirectories.Length);
        }

        [TestMethod]
        public void OpenRootSubDirectories_NameIsValid()
        {
            var Files = new DirectoryHierarchy();
            Files.OpenDirectory(RootDirectoryName);
            var SubDirectories = Files.OpenDirectory(Files.RootDirectory);
            Assert.AreEqual("Kevin MacLeod", SubDirectories[0].Name, false);
        }

        [TestMethod]
        [DynamicData(nameof(AllDirectoriesNameTree))]
        public void OpenAllDirectories_NamesAreValid(FileNameTree ExpectedNameTree)
        {
            var Files = new DirectoryHierarchy();
            Files.OpenDirectory(RootDirectoryName);

            void AssertAndOpen(DirectoryInfo Directory, FileNameTree SubTree)
            {
                Assert.AreEqual(SubTree.Name, Directory.Name, false);
                var Subdirectories = Files.OpenDirectory(Directory);
                for (int i = 0; i < Subdirectories.Length; i++)
                {
                    AssertAndOpen(Subdirectories[i], SubTree.Children[i]);
                }
            }
            AssertAndOpen(Files.RootDirectory, ExpectedNameTree);
        }

        [TestMethod]
        public void OpenAllDirectories_CloseRootSubDirectories_DirectoryCountIsOne()
        {
            var Files = new DirectoryHierarchy();
            Files.OpenDirectory(RootDirectoryName);

            void Open(DirectoryInfo Directory)
            {
                foreach (var Subdirectory in Files.OpenDirectory(Directory))
                {
                    Open(Subdirectory);
                }
            }
            Open(Files.RootDirectory);
            Files.CloseSubDirectories(Files.RootDirectory);

            Assert.AreEqual(1, Files.GetAllOpenDirectories().Length);
        }

        [TestMethod]
        public void OpenAllDirectories_CloseInReverse_DirectoryCountIsOne()
        {
            var Files = new DirectoryHierarchy();
            Files.OpenDirectory(RootDirectoryName);

            var CloseQueue = new List<DirectoryInfo>();
            void Open(DirectoryInfo Directory)
            {
                CloseQueue.Insert(0, Directory);
                foreach (var Subdirectory in Files.OpenDirectory(Directory))
                {
                    Open(Subdirectory);
                }
            }
            Open(Files.RootDirectory);
            foreach (var Directory in CloseQueue)
            {
                Files.CloseSubDirectories(Directory);
            }

            Assert.IsTrue(Files.GetAllOpenDirectories().Length == 1);
        }

        [TestMethod]
        public void OpenRootCloseRoot_ThrowArgumentException()
        {
            var Files = new DirectoryHierarchy();
            Files.OpenDirectory(RootDirectoryName);
            try
            {
                Files.CloseDirectory(Files.RootDirectory);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
            }
        }
    }
}
