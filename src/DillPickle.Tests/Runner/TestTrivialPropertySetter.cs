using DillPickle.Framework.Exceptions;
using DillPickle.Framework.Runner;
using NUnit.Framework;

namespace DillPickle.Tests.Runner
{
    [TestFixture]
    public class TestTrivialPropertySetter : FixtureBase
    {
        static TrivialPropertySetter sut;

        public override void DoSetUp()
        {
            sut = new TrivialPropertySetter();
        }

        [Test]
        public void SetsPropertyOnInstanceLikeExpected()
        {
            var instance = new SomeClass();

            // propertyof(SomeClass.SomeProperty) would be nice...
            sut.SetValue(instance, typeof(SomeClass).GetProperty("SomeProperty"), "yo!");

            Assert.AreEqual("yo!", instance.SomeProperty);
        }

        [Test]
        public void DoesTrivialConversionsNoQuestionsAsked()
        {
            var instance = new SomeClass();

            sut.SetValue(instance, typeof(SomeClass).GetProperty("AnotherProperty"), "42");
            sut.SetValue(instance, typeof(SomeClass).GetProperty("YetAnotherProperty"), "42,5");
            sut.SetValue(instance, typeof(SomeClass).GetProperty("YetYetAnotherProperty"), "67,7");
            
            Assert.AreEqual(42, instance.AnotherProperty);
            Assert.AreEqual(42.5, instance.YetAnotherProperty);
            Assert.AreEqual(67.7, instance.YetYetAnotherProperty);
        }

        [Test]
        public void ThrowsExceptionWithNiceMessageWhenTrivialConversionCouldNotBeCompleted()
        {
            var instance = new SomeClass();
            
            var ex = Assert.Throws<FeatureExecutionException>(() => sut.SetValue(instance, typeof(SomeClass).GetProperty("AnotherProperty"), "yo!"));

            Assert.AreEqual("The value 'yo!' could not be automatically converted to target type Int32 (AnotherProperty property of SomeClass)", ex.Message);
        }

        class SomeClass
        {
            public string SomeProperty { get; set; }
            
            public int AnotherProperty { get; set; }
            public double YetAnotherProperty { get; set; }
            public decimal YetYetAnotherProperty { get; set; }
        }
    }
}