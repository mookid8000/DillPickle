using DillPickle.Framework.Parser.Api;
using DillPickle.Framework.Runner;
using NUnit.Framework;

namespace DillPickle.Tests.Parser.Api
{
    [TestFixture]
    public class TestFeature : FixtureBase
    {
        [Test]
        public void FeatureKnowsWhenItsIncludedAndExcluded_NoTagsOnFeature()
        {
            var feature = new Feature("test", new string[0])
                              {
                                  Scenarios =
                                      {
                                          new ExecutableScenario("test1", new string[0]),
                                          new ExecutableScenario("test2", new string[0]),
                                      }
                              };

            Assert.IsTrue(feature.ShouldBeIncluded(TagFilter.Empty()));
            Assert.IsFalse(feature.ShouldBeIncluded(new TagFilter(new[] {"a"}, new string[0])));
            Assert.IsTrue(feature.ShouldBeIncluded(new TagFilter(new string[0], new[] {"a"})));
        }

        [Test]
        public void FeatureKnowsWhenItsIncludedAndExcluded_TagsOnFeature()
        {
            var feature = new Feature("test", new []{"a"})
                              {
                                  Scenarios =
                                      {
                                          new ExecutableScenario("test1", new string[0]),
                                          new ExecutableScenario("test2", new string[0]),
                                      }
                              };

            Assert.IsTrue(feature.ShouldBeIncluded(TagFilter.Empty()));
            Assert.IsTrue(feature.ShouldBeIncluded(new TagFilter(new[] {"a"}, new string[0])));
            Assert.IsFalse(feature.ShouldBeIncluded(new TagFilter(new[] {"b"}, new string[0])));
            Assert.IsFalse(feature.ShouldBeIncluded(new TagFilter(new string[0], new[] {"a"})));
            Assert.IsTrue(feature.ShouldBeIncluded(new TagFilter(new string[0], new[] {"b"})));
        }

        [Test]
        public void FeatureKnowsWhenItsIncludedAndExcluded_TagsOnScenario()
        {
            var feature = new Feature("test", new string[0])
                              {
                                  Scenarios =
                                      {
                                          new ExecutableScenario("test1", new string[0]),
                                          new ExecutableScenario("test2", new[] {"a"}),
                                      }
                              };

            Assert.IsTrue(feature.ShouldBeIncluded(TagFilter.Empty()));
            
            var inclusionScenarioTagFilter = new TagFilter(new[] {"a"}, new string[0]);
            Assert.IsTrue(feature.ShouldBeIncluded(inclusionScenarioTagFilter));
            Assert.IsFalse(feature.Scenarios[0].ShouldBeIncluded(inclusionScenarioTagFilter));
            Assert.IsTrue(feature.Scenarios[1].ShouldBeIncluded(inclusionScenarioTagFilter));
            
            Assert.IsFalse(feature.ShouldBeIncluded(new TagFilter(new[] {"b"}, new string[0])));

            var exclusionScenarioTagFilter = new TagFilter(new string[0], new[] {"a"});
            Assert.IsTrue(feature.ShouldBeIncluded(exclusionScenarioTagFilter));
            Assert.IsTrue(feature.Scenarios[0].ShouldBeIncluded(exclusionScenarioTagFilter));
            Assert.IsFalse(feature.Scenarios[1].ShouldBeIncluded(exclusionScenarioTagFilter));
            
            Assert.IsTrue(feature.ShouldBeIncluded(new TagFilter(new string[0], new[] {"b"})));
        }
    }
}