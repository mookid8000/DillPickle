using DillPickle.Framework.Runner;
using NUnit.Framework;

namespace DillPickle.Tests.Runner
{
    [TestFixture]
    public class TestTagFilter
    {
        [Test]
        public void EmptyFilterIncludesEverythingExcludesNothing()
        {
            Assert.IsTrue(new TagFilter(new string[0], new string[0]).Includes(new[] {"current", "bam"}));
            Assert.IsFalse(new TagFilter(new string[0], new string[0]).Excludes(new[] {"current", "bam"}));
        }

        [Test]
        public void FilterInclusionWorks()
        {
            Assert.IsTrue(new TagFilter(new[] {"current", "bam"}, new string[0]).Includes(new[] {"current", "bam"}));
            Assert.IsTrue(new TagFilter(new[] {"current"}, new string[0]).Includes(new[] {"current", "bam"}));
            Assert.IsFalse(new TagFilter(new[] {"something_else"}, new string[0]).Includes(new[] {"current", "bam"}));
        }

        [Test]
        public void FilterExclusionWorks()
        {
            Assert.IsTrue(new TagFilter(new string[0], new[] {"current", "bam"}).Excludes(new[] {"current", "bam"}));
            Assert.IsTrue(new TagFilter(new string[0], new[] {"current"}).Excludes(new[] {"current", "bam"}));
            Assert.IsFalse(new TagFilter(new string[0], new[] {"something_else"}).Excludes(new[] {"current", "bam"}));
        }

        [Test]
        public void CanCompareFilters()
        {
            AssertEqual(TagFilter.Empty(),
                        new TagFilter(new string[0], new string[0]));

            AssertEqual(new TagFilter(new[] {"tag1", "tag2"}, new[] {"tag3", "tag4"}),
                        new TagFilter(new[] {"tag2", "tag1"}, new[] {"tag4", "tag3"}));
        }

        void AssertEqual(TagFilter filter1, TagFilter filter2)
        {
            Assert.IsTrue(filter1 == filter2);
            Assert.IsFalse(filter1 != filter2);
        }
    }
}