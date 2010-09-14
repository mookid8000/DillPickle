using NUnit.Framework;
using Rhino.Mocks;

namespace DillPickle.Tests
{
    public class FixtureBase
    {
        protected const string TempPath = @"c:\temp";

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            DoTestFixtureSetUp();
        }

        [SetUp]
        public void SetUp()
        {
            DoSetUp();
        }

        [TearDown]
        public void TearDown()
        {
            DoTearDown();
        }

        public virtual void DoTestFixtureSetUp()
        {
        }

        public virtual void DoSetUp()
        {
        }

        public virtual void DoTearDown()
        {
        }

        protected T Mock<T>() where T : class
        {
            return MockRepository.GenerateMock<T>();
        }
    }
}