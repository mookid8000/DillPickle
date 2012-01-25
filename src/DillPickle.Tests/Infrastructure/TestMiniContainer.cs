using System;
using DillPickle.Framework.Infrastructure;
using NUnit.Framework;

namespace DillPickle.Tests.Infrastructure
{
    [TestFixture]
    public class TestMiniContainer : FixtureBase
    {
        MiniContainer container;

        public override void DoSetUp()
        {
            container = new MiniContainer();
        }

        [Test]
        public void WillInstantiateConcreteTypeWithoutTypeMapping()
        {
            var instance = container.Resolve<RandomType>();

            Assert.IsNotNull(instance);
        }

        class RandomType { }

        [Test]
        public void CanInstantiateServiceWhenTypeMappingIsPresent()
        {
            container.MapType<ISomeInterface, SomeType>();

            var instance = container.Resolve<ISomeInterface>();

            Assert.IsNotNull(instance);
            Assert.AreEqual(typeof(SomeType), instance.GetType());
        }

        interface ISomeInterface { }
        class SomeType : ISomeInterface { }

        [Test]
        public void CanInstantiateStuffRecursively()
        {
            var instance = container.Resolve<Scenario01.FirstLevelDependency>();

            Assert.IsNotNull(instance);
        }

        class Scenario01 {
            public class ThirdLevelDependency { }
            public class SecondLevelDependency { public SecondLevelDependency(ThirdLevelDependency t) { } }
            public class FirstLevelDependency { public FirstLevelDependency(SecondLevelDependency t) { } }
        }

        [Test]
        public void CanInstantiateStuffRecursivelyWithTypeMappingsAndAll()
        {
            container.MapType<Scenario02.IFirstLevelDependency, Scenario02.FirstLevelDependency>()
                .MapType<Scenario02.ISecondLevelDependency, Scenario02.SecondLevelDependency>()
                .MapType<Scenario02.IThirdLevelDependency, Scenario02.ThirdLevelDependency>();

            var instance = container.Resolve<Scenario02.IFirstLevelDependency>();

            Assert.AreEqual(typeof(Scenario02.FirstLevelDependency), instance.GetType());
            Assert.AreEqual(typeof(Scenario02.SecondLevelDependency), instance.Dep.GetType());
            Assert.AreEqual(typeof(Scenario02.ThirdLevelDependency), instance.Dep.Dep.GetType());
        }

        class Scenario02
        {
            public interface IThirdLevelDependency { }
            
            public interface ISecondLevelDependency {
                IThirdLevelDependency Dep { get; set; }
            }
            public interface IFirstLevelDependency {
                ISecondLevelDependency Dep { get; set; }
            }

            public class ThirdLevelDependency : IThirdLevelDependency { }
            
            public class SecondLevelDependency : ISecondLevelDependency
            {
                public IThirdLevelDependency Dep { get; set; }

                public SecondLevelDependency(IThirdLevelDependency dep)
                {
                    Dep = dep;
                }
            }
            
            public class FirstLevelDependency : IFirstLevelDependency
            {
                public ISecondLevelDependency Dep { get; set; }

                public FirstLevelDependency(ISecondLevelDependency dep)
                {
                    Dep = dep;
                }
            }
        }

        [Test]
        public void WillReadilyDecorateLikeWindsor()
        {
            container.MapType<Scenario03.ISomeInterface, Scenario03.SomeDecoratingImplementation>()
                .MapType<Scenario03.ISomeInterface, Scenario03.SomeImplementation>();

            var instance = container.Resolve<Scenario03.ISomeInterface>();

            Assert.AreEqual("decorated message from some implementation", instance.GetMessage());
        }

        class Scenario03
        {
            public interface ISomeInterface
            {
                string GetMessage();
            }

            public class SomeImplementation : ISomeInterface
            {
                public string GetMessage()
                {
                    return "message from some implementation";
                }
            }

            public class SomeDecoratingImplementation : ISomeInterface
            {
                readonly ISomeInterface somethingToDecorate;

                public SomeDecoratingImplementation(ISomeInterface somethingToDecorate)
                {
                    this.somethingToDecorate = somethingToDecorate;
                }

                public string GetMessage()
                {
                    return "decorated " + somethingToDecorate.GetMessage();
                }
            }
        }

        [Test]
        public void ThrowsWithNiceMessageWhenAttemptingToRegisterInterfaceOrAbstractTypeAsImplementation()
        {
            var e1 = Assert.Throws<InvalidOperationException>(() => container.MapType<Scenario04.INotConcrete, Scenario04.INotConcrete>());
            var e2 = Assert.Throws<InvalidOperationException>(() => container.MapType<Scenario04.NotConcrete, Scenario04.NotConcrete>());

            Assert.That(e1.Message, Contains.Substring(typeof(Scenario04.INotConcrete).ToString()));
            Assert.That(e2.Message, Contains.Substring(typeof(Scenario04.NotConcrete).ToString()));
        }

        class Scenario04
        {
            public interface INotConcrete {}
            public abstract class NotConcrete {}
        }

        [Test]
        public void CanConfigureParticularObjectIfNecessary()
        {
            container.MapType<Scenario05.IDep, Scenario05.Impl>()
                .MapType<Scenario05.IAnotherDep, Scenario05.AnotherImpl>();

            container.Configure<Scenario05.Impl>(i => i.StringProperty = "yo!")
                .Configure<Scenario05.AnotherImpl>(i => i.Initialize());

            var root = container.Resolve<Scenario05.Root>();

            var strings = root.GetStrings();
            Assert.AreEqual("StringProperty: yo!, another string: initialized!", strings);
        }

        class Scenario05
        {
            public class Root
            {
                readonly IDep dep;

                public Root(IDep dep)
                {
                    this.dep = dep;
                }

                public string GetStrings()
                {
                    return dep.GetStrings();
                }
            }

            public interface IDep
            {
                string GetStrings();
            }

            public interface IAnotherDep
            {
                string GetStrings();
            }

            public class Impl : IDep
            {
                readonly IAnotherDep anotherDep;

                public Impl(IAnotherDep anotherDep)
                {
                    this.anotherDep = anotherDep;
                }

                public string StringProperty { get; set; }
                
                public string GetStrings()
                {
                    return string.Format("StringProperty: {0}, another string: {1}", StringProperty, anotherDep.GetStrings());
                }
            }

            public class AnotherImpl : IAnotherDep
            {
                string text;

                public void Initialize()
                {
                    text = "initialized!";
                }

                public string GetStrings()
                {
                    return text;
                }
            }
        }
    }

}