using System.Collections.Generic;
using System.IO;
using System.Linq;
using DillPickle.Framework.Exceptions;
using DillPickle.Framework.Runner;
using NUnit.Framework;

namespace DillPickle.Tests.Runner
{
    [TestFixture]
    public class TestFeatureFileFinder : FixtureBase
    {
        List<string> directories;

        FeatureFileFinder sut;

        public override void DoSetUp()
        {
            directories = new List<string>();
            sut = new FeatureFileFinder();
        }

        public override void DoTearDown()
        {
            directories.ForEach(f => Directory.Delete(f, true));
        }

        public void CreateDirectory(string path)
        {
            if (Directory.Exists(path)) Assert.Fail("Directory {0} already exists!!", path);
            Directory.CreateDirectory(path);
            directories.Add(path);
        }

        [Test]
        public void ThrowsExceptionWithNiceMessageIfPointedTowardNonexistingDirectory()
        {
            var ex = Assert.Throws<FeatureExecutionException>(() => sut.Find(@"c:\temp\this-directory-does-most-likely-not-exist\*.feature"));

            Assert.IsTrue(ex.Message.Contains(@"c:\temp\this-directory-does-most-likely-not-exist"),
                "Exception message does not seem to contain path information: {0}", ex.Message);
        }

        [Test]
        public void FindsEmptyListWhenPointedTowardsEmptyDirectory()
        {
            var directory = Path.Combine(TempPath, "some-random-directory");
            CreateDirectory(directory);

            var files = sut.Find(Path.Combine(directory, "*.feature"));

            Assert.IsFalse(files.Any());
        }

        [Test]
        public void CanFindFeatureFilesLikeExpected()
        {
            var directory = Path.Combine(TempPath, "some-random-directory");
            CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "file1.feature"), "yo!");
            File.WriteAllText(Path.Combine(directory, "file2.features"), "yo!");
            File.WriteAllText(Path.Combine(directory, "file3.feature"), "yo!");

            var files = sut.Find(Path.Combine(directory, "*.feature")).OrderBy(f => f).ToList();
            
            Assert.AreEqual(2, files.Count);
            Assert.AreEqual(Path.Combine(directory, "file1.feature"), files[0]);
            Assert.AreEqual(Path.Combine(directory, "file3.feature"), files[1]);
        }
    }
}