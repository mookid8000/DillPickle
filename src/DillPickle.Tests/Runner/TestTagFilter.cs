using DillPickle.Framework.Runner;
using NUnit.Framework;

namespace DillPickle.Tests.Runner
{
    [TestFixture]
    public class TestTagFilter
    {
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