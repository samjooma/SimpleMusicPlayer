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
            var Files = new FileHierarchy(RootDirectoryName);
            Assert.AreEqual(RootDirectoryName, Files.RootDirectory.Name, false);
        }

        [TestMethod]
        public void GetRootSubDirectories_LengthIsOne()
        {
            var Files = new FileHierarchy(RootDirectoryName);
            var asdf = Files.OpenDirectory(Files.RootDirectory);
            Assert.AreEqual(1, asdf.Length);
        }

        [TestMethod]
        public void GetRootSubDirectories_NameIsValid()
        {
            var Files = new FileHierarchy(RootDirectoryName);
            var SubDirectories = Files.OpenDirectory(Files.RootDirectory);
            Assert.AreEqual("Kevin MacLeod", SubDirectories[0].Name, false);
        }
    }
}
