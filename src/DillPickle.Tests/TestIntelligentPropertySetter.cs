using System;
using System.Reflection;
using DillPickle.Framework.Runner;
using DillPickle.Framework.Runner.Api;
using NUnit.Framework;
using Rhino.Mocks;

namespace DillPickle.Tests
{
    [TestFixture]
    public class TestIntelligentPropertySetter : FixtureBase
    {
        IPropertySetter fallbackPropertySetter;
        IntelligentPropertySetter sut;

        public override void DoSetUp()
        {
            fallbackPropertySetter = Mock<IPropertySetter>();
            sut = new IntelligentPropertySetter(fallbackPropertySetter, new TrivialObjectActivator());
            sut.AddAssembly(Assembly.GetExecutingAssembly());
        }

        [Test]
        public void CanSetPropertyWithConverterLikeExpected()
        {
            var instance = new SomeClass();

            sut.SetValue(instance, typeof(SomeClass).GetProperty("DateTime"), "2010-09-03 11:03:30");

            Assert.AreEqual(new DateTime(2010, 9, 3, 11, 03, 30), instance.DateTime);
        }

        [Test]
        public void InvokesFallbackSetterIfConverterIsUnknown()
        {
            var instance = new SomeClass();
            var propertyInfo = typeof(SomeClass).GetProperty("Uri");
            var someString = "2010-09-03 13:58:23";
            
            sut.SetValue(instance, propertyInfo, someString);

            fallbackPropertySetter.AssertWasCalled(s => s.SetValue(instance, propertyInfo, someString));
        }

        class SomeClass
        {
            public DateTime DateTime { get; set; }
            public Uri Uri { get; set; }
        }

        [TypeConverter]
        class DateTimeConverter : ITypeConverter<DateTime>
        {
            public DateTime Convert(string value)
            {
                return DateTime.Parse(value);
            }
        }
    }
}