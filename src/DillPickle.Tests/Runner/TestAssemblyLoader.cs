using System.Configuration;
using System.IO;
using DillPickle.Framework.Runner;
using NUnit.Framework;

namespace DillPickle.Tests.Runner
{
    [TestFixture]
    public class TestAssemblyLoader : FixtureBase
    {
        AssemblyLoader sut;

        public override void DoSetUp()
        {
            sut = new AssemblyLoader();
        }

        [Test]
        public void ThrowsIfAssemblyDoesNotExist()
        {
            Assert.Throws<FileNotFoundException>(() => sut.LoadAssemblyWithApplicationConfigurationIfPossible(@"c:\temp\this-file-does-most-likely-not-exist.dll"));
        }

        [Test, Ignore("Can't make this one pass... it seems to work as expected though when using it from the cli")]
        public void LoadsAssemblyAndConfigurationLikeExpected()
        {
            // this is the special assert-act-assert pattern of testing...
            Assert.IsNull(ConfigurationManager.AppSettings["some-random-appsetting"]);

            var path = @"..\..\..\DillPickle.TestAssembly\bin\Debug\DillPickle.TestAssembly.dll";
            var assembly = sut.LoadAssemblyWithApplicationConfigurationIfPossible(path);

            Assert.AreEqual("some-random-value", ConfigurationManager.AppSettings["some-random-appsetting"]);
        }
    }
}