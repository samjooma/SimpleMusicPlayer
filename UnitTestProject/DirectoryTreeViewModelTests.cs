using SimpleMusicPlayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleMusicPlayer.ViewModel;

namespace UnitTestProject
{
    [TestClass]
    [DeploymentItem("Test files", "Test files")]
    public class DirectoryTreeViewModelTests
    {
        public class NameTreeNode
        {
            public string Name;
            public NameTreeNode[] Children;
            public NameTreeNode(string Name, IEnumerable<NameTreeNode> Children)
            {
                this.Name = Name;
                this.Children = Children.ToArray();
            }
            public NameTreeNode(string Name) : this(Name, Array.Empty<NameTreeNode>()) { }
        }

        private const string RootDirectoryName = "Test files";
        public static NameTreeNode AllDirectoriesNameTree // TODO: Make this a JSON or XML file.
        {
            get =>
            new NameTreeNode(RootDirectoryName, [
                new("Kevin MacLeod", [
                    new("Ambient"),
                    new("Electronic"),
                    new("Other")
                ]),
                new("Public domain", [
                    new("1920s", [
                        new("a", [
                            new("b", [
                                new("c"),
                                new("d"),
                                new("e")
                            ])
                        ]),
                        new("stuff"),
                    ]),
                    new("1926", [
                        new("Virginia Liston")
                    ]),
                    new("1927", [
                        new("Blind Lemon Jefferson"),
                        new("Burnett And Rutherford"),
                        new("Dock Boggs"),
                        new("G B Grayson"),
                        new("Kelly Harrell", [
                            new("Kelly Harrell And The Virginia String Band")
                        ])
                    ])
                ])
            ]);
        }

        private void ExpandRecursively(DirectoryNode Directory)
        {
            Directory.IsExpanded = true;
            if (Directory.Children != null)
            {
                foreach (DirectoryNode Child in Directory.Children.OfType<DirectoryNode>())
                {
                    ExpandRecursively(Child);
                }
            }
        }

        private List<FileContainerNode> GetAllNodesRecursively(FileContainerNode ContainerNode)
        {
            List<FileContainerNode> Result = [ContainerNode];
            if (ContainerNode is DirectoryNode DirectoryNode)
            {
                foreach (FileContainerNode Child in DirectoryNode.Children)
                {
                    Result.AddRange(GetAllNodesRecursively(Child));
                }
            }
            return Result;
        }

        //
        // Test methods.
        //

        [TestMethod]
        public void CreateViewModel_RootIsNotNull()
        {
            DirectoryTree DirectoryTree = new ();
            DirectoryTree.AddDirectory(new DirectoryInfo(RootDirectoryName));
            Assert.IsNotNull(DirectoryTree.RootDirectory);
        }

        [TestMethod]
        public void CreateViewModel_RootNameIsValid()
        {
            DirectoryTree DirectoryTree = new ();
            DirectoryTree.AddDirectory(new DirectoryInfo(RootDirectoryName));
            Assert.AreEqual(DirectoryTree.RootDirectory?.Name, RootDirectoryName, false);
        }

        [TestMethod]
        public void CreateViewModel_RootChildrenIsNotNull()
        {
            DirectoryTree DirectoryTree = new ();
            DirectoryTree.AddDirectory(new DirectoryInfo(RootDirectoryName));
            Assert.IsNotNull(DirectoryTree.RootDirectory?.Children);
        }

        [TestMethod]
        public void ExpandAll_RecursiveChildrenAreNotNull()
        {
            DirectoryTree DirectoryTree = new ();
            DirectoryTree.AddDirectory(new DirectoryInfo(RootDirectoryName));
            ExpandRecursively(DirectoryTree.RootDirectory!);

            void AssertChildrenAreNotNull(DirectoryNode Directory)
            {
                Assert.IsNotNull(Directory.Children);
                foreach (DirectoryNode Child in Directory.Children.OfType<DirectoryNode>())
                {
                    AssertChildrenAreNotNull(Child);
                }
            }
            AssertChildrenAreNotNull(DirectoryTree.RootDirectory!);
        }

        [TestMethod]
        public void ExpandAll_TreeContainsAllTestDirectoryNames()
        {
            DirectoryTree DirectoryTree = new ();
            DirectoryTree.AddDirectory(new DirectoryInfo(RootDirectoryName));
            ExpandRecursively(DirectoryTree.RootDirectory!);

            void AssertChildrenNamesAreEqual(DirectoryNode DirectoryNode, NameTreeNode NameNode)
            {
                HashSet<string> SetA = new(DirectoryNode.Children.Select(x => x.Name));
                HashSet<string> SetB = new(NameNode.Children.Select(x => x.Name));
                Assert.IsTrue(SetA.SetEquals(SetB));

                foreach (DirectoryNode ChildDirectory in DirectoryNode.Children)
                {
                    NameTreeNode ChildName = NameNode.Children.First(x => x.Name == ChildDirectory.Name);
                    AssertChildrenNamesAreEqual(ChildDirectory, ChildName);
                }
            }
            Assert.AreEqual(AllDirectoriesNameTree.Name, DirectoryTree.RootDirectory!.Name);
            AssertChildrenNamesAreEqual(DirectoryTree.RootDirectory!, AllDirectoriesNameTree);
        }

        [TestMethod]
        public void ExpandAllAndShrinkRoot_RootContainsTwoChildren()
        {
            DirectoryTree DirectoryTree = new ();
            DirectoryTree.AddDirectory(new DirectoryInfo(RootDirectoryName));
            ExpandRecursively(DirectoryTree.RootDirectory!);

            DirectoryTree.RootDirectory!.IsExpanded = false;
            Assert.AreEqual(2, DirectoryTree.RootDirectory!.Children.Count());
        }

        [TestMethod]
        public void TrySelectingAllDirectories_OnlyOneIsSelected()
        {
            DirectoryTree DirectoryTree = new ();
            DirectoryTree.AddDirectory(new DirectoryInfo(RootDirectoryName));

            void SelectRecursively(FileContainerNode ContainerNode)
            {
                ContainerNode.IsSelected = true;
                if (ContainerNode is DirectoryNode DirectoryNode)
                {
                    DirectoryNode.IsExpanded = true;
                    foreach (FileContainerNode Child in DirectoryNode.Children)
                    {
                        SelectRecursively(Child);
                    }
                }
            }
            SelectRecursively(DirectoryTree.RootDirectory!);

            List<FileContainerNode> AllNodes = GetAllNodesRecursively(DirectoryTree.RootDirectory!);
            int TotalSelectedCount = AllNodes.Count(x => x.IsSelected);
            Assert.AreEqual(1, TotalSelectedCount);
        }

        [TestMethod]
        public void TrySelectingAllDirectories_SelectedReferenceIsCorrect()
        {
            DirectoryTree DirectoryTree = new();
            DirectoryTree.AddDirectory(new DirectoryInfo(RootDirectoryName));

            void SelectRecursively(FileContainerNode ContainerNode)
            {
                ContainerNode.IsSelected = true;
                if (ContainerNode is DirectoryNode DirectoryNode)
                {
                    DirectoryNode.IsExpanded = true;
                    foreach (FileContainerNode Child in DirectoryNode.Children)
                    {
                        SelectRecursively(Child);
                    }
                }
            }
            SelectRecursively(DirectoryTree.RootDirectory!);

            List<FileContainerNode> AllNodes = GetAllNodesRecursively(DirectoryTree.RootDirectory!);
            FileContainerNode? FoundSelected = AllNodes.Find(x => x.IsSelected);
            Assert.AreSame(FoundSelected, DirectoryTree.SelectedNode);
        }
    }
}
