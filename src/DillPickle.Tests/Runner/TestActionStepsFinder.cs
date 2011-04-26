using System;
using System.Reflection;
using DillPickle.Framework.Exceptions;
using DillPickle.Framework.Executor.Attributes;
using DillPickle.Framework.Runner;
using DillPickle.Framework.Runner.Api;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Rhino.Mocks;

namespace DillPickle.Tests.Runner
{
    [TestFixture]
    public class TestActionStepsFinder : FixtureBase
    {
        ActionStepsFinder finder;
        IAssemblyLoader assemblyLoader;

        public override void DoSetUp()
        {
            assemblyLoader = Mock<IAssemblyLoader>();
            assemblyLoader.Stub(l => l.LoadConfiguredAssembly(Arg<string>.Is.Anything))
                .Return(Assembly.GetExecutingAssembly());
            
            finder = new ActionStepsFinder(assemblyLoader);
        }

        [Test]
        public void FindsActionStepsClassesLikeExpected()
        {
            var types = finder.FindTypesWithActionSteps("does not matter", "ShouldMatchFirstCase.feature");

            Assert.That(types, Has.Length.EqualTo(2));
            Assert.That(types, Contains.Item(typeof(ShouldMatchFirstCase)));
            Assert.That(types, Contains.Item(typeof(ShouleBeIncluded)));
        }

        [Test]
        public void FindsActionStepsClassesLikeExpectedWithAlternativeNaming()
        {
            var types = finder.FindTypesWithActionSteps("does not matter", "should_match_second_case.feature");

            Assert.That(types, Has.Length.EqualTo(2));
            Assert.That(types, Contains.Item(typeof(should_match_second_case)));
            Assert.That(types, Contains.Item(typeof(ShouleBeIncluded)));
        }
        
        [Test]
        public void ThrowsWhenAmbiguousMatchIsFound()
        {
            var ambiguousNames = new[] {"ShouldThrowBecauseOfAmbiguousMatch", "should_throw_because_of_ambiguous_match"};

            foreach(var name in ambiguousNames)
            {
                var nameToTry = name;
                var ex = Assert.Throws<FeatureExecutionException>(() => finder.FindTypesWithActionSteps("does not matter", nameToTry));
                
                Assert.That(ex.Message, Contains.Substring("ambiguous"));

                // names of all ambiguously named classes should be mentioned in the error message
                foreach (var nameToIncludeInErrorMessage in ambiguousNames)
                {
                    Assert.That(ex.Message, Contains.Substring(nameToIncludeInErrorMessage));
                }                
            }
        }

        [Test]
        public void ThrowsWhenNoMatchIsFound()
        {
            var ex = Assert.Throws<FeatureExecutionException>(() => finder.FindTypesWithActionSteps("does not matter", "classByThisNameDoesNotExist"));

            Assert.That(ex.Message, Contains.Substring("classByThisNameDoesNotExist"));
        }

        [ActionSteps]
        class ShouldThrowBecauseOfAmbiguousMatch { }

        [ActionSteps]
        class should_throw_because_of_ambiguous_match { }

        [ActionSteps]
        [IncludeActionSteps(typeof(ShouleBeIncluded))]
        class ShouldMatchFirstCase { }

        [ActionSteps]
        [IncludeActionSteps(typeof(ShouleBeIncluded))]
        class should_match_second_case {}

        [ActionSteps]
        class ShouleBeIncluded {}

        [ActionSteps]
        class ShouleNotBeIncluded {}
    }
}