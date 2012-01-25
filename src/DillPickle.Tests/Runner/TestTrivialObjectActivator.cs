using DillPickle.Framework.Runner;
using NUnit.Framework;

namespace DillPickle.Tests.Runner
{
    [TestFixture]
    public class TestTrivialObjectActivator : FixtureBase
    {
        TrivialObjectActivator sut;

        public override void DoSetUp()
        {
            sut = new TrivialObjectActivator();
        }

        [Test]
        public void CanInstantiateStuff()
        {
            var instance = sut.GetInstance(typeof(SomeClass));

            Assert.AreEqual(typeof(SomeClass), instance.GetType());
        }

        [Test]
        public void GeneratesNewInstanceEveryTime()
        {
            var instance = sut.GetInstance(typeof(SomeClass));
            var nextInstance = sut.GetInstance(typeof(SomeClass));

            Assert.AreNotSame(instance, nextInstance);
        }

        class SomeClass
        {
        }
    }
}