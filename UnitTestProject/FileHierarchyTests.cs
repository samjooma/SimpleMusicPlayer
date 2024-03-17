using MyMusicPlayer.Model;
using MyMusicPlayer.ViewModel;

namespace UnitTestProject
{
    [TestClass]
    [DeploymentItem("Test files", "Test files")]
    public class FileHierarchyTests
    {
        private const string RootDirectoryName = "Test files";

        [TestMethod]
        public void CreateRoot_NameIsValid()
        {
            var Files = new FileHierarchy();
            Files.OpenDirectory(RootDirectoryName);
            Assert.AreEqual(RootDirectoryName, Files.RootDirectory.Name, false);
        }

        [TestMethod]
        public void OpenRootSubDirectories_LengthIsOne()
        {
            var Files = new FileHierarchy();
            Files.OpenDirectory(RootDirectoryName);
            var SubDirectories = Files.OpenDirectory(Files.RootDirectory);
            Assert.AreEqual(1, SubDirectories.Length);
        }

        [TestMethod]
        public void OpenRootSubDirectories_NameIsValid()
        {
            var Files = new FileHierarchy();
            Files.OpenDirectory(RootDirectoryName);
            var SubDirectories = Files.OpenDirectory(Files.RootDirectory);
            Assert.AreEqual("Kevin MacLeod", SubDirectories[0].Name, false);
        }
    }
}
