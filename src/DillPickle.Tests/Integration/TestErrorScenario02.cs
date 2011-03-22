using System.Collections.Generic;
using DillPickle.Framework.Executor.Attributes;
using DillPickle.Framework.Parser;
using DillPickle.Framework.Runner;
using NUnit.Framework;

namespace DillPickle.Tests.Integration
{
    [TestFixture]
    public class TestErrorScenario02 : FixtureBase
    {
        [Test]
        public void Something()
        {
            var parser = new StupidGherkinParser();
            var result = parser.Parse(@"

Scenario: Error!

Then we have the following signals:
    | Tag | Value |
    | REG_T1.001 | 9.75 kW |

Then we have the following signals:
    | Tag | Value |
    | REG_T1.001 | 8 kW |

Then we have the following signals:
    | Tag | Value |
    | REG_T1.001 | 7 kW |
");

            var activator = new TrivialObjectActivator();
            var runner = new FeatureRunner(activator,
                                           new IntelligentPropertySetter(new TrivialPropertySetter(), activator));

            result.Features.ForEach(f => runner.Run(f, new[] { typeof(Steps) }, NullFilter()));

            Assert.AreEqual(3, Steps.Calls.Count);

            AssertSignals(Steps.Calls[0], "REG_T1.001", "9.75 kW");
            AssertSignals(Steps.Calls[1], "REG_T1.001", "8 kW");
            AssertSignals(Steps.Calls[2], "REG_T1.001", "7 kW");
        }

        void AssertSignals(Steps.TypedSignals typedSignals, string expectedTag, string expectedValue)
        {
            Assert.AreEqual(expectedTag, typedSignals.Tag);
            Assert.AreEqual(expectedValue, typedSignals.Value);
        }

        [ActionSteps]
        class Steps
        {
            public static List<TypedSignals> Calls = new List<TypedSignals>();

            [Then("we have the following signals:")]
            public void SomeMethod(TypedSignals[] signals)
            {
                Calls.AddRange(signals);
            }

            public class TypedSignals
            {
                public string Tag { get; set; }
                public string Value { get; set; }
            }
        }

        RunnerOptions NullFilter()
        {
            return new RunnerOptions
                       {
                           Filter = TagFilter.Empty(),
                           DruRun = false,
                       };
        }
    }
}