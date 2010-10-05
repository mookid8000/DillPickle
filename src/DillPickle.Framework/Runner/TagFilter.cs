using System;
using System.Collections.Generic;
using System.Linq;

namespace DillPickle.Framework.Runner
{
    public class TagFilter : IEquatable<TagFilter>
    {
        readonly string[] tagsToInclude;
        readonly string[] tagsToExclude;

        public TagFilter(string[] tagsToInclude, string[] tagsToExclude)
        {
            this.tagsToInclude = tagsToInclude;
            this.tagsToExclude = tagsToExclude;
        }

        public bool IsSatisfiedBy(IEnumerable<string> tags)
        {
            var yes = true;

            yes &= tagsToInclude.Intersect(tags).Any() || !tagsToInclude.Any();
            yes &= !tagsToExclude.Intersect(tags).Any() || !tagsToExclude.Any();

            return yes;
        }

        public bool Equals(TagFilter other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            
            return !other.tagsToInclude.Except(tagsToInclude).Any()
                   && !other.tagsToExclude.Except(tagsToExclude).Any();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (TagFilter)) return false;
            return Equals((TagFilter) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((tagsToInclude != null ? tagsToInclude.GetHashCode() : 0)*397) ^ (tagsToExclude != null ? tagsToExclude.GetHashCode() : 0);
            }
        }

        public static bool operator ==(TagFilter left, TagFilter right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TagFilter left, TagFilter right)
        {
            return !Equals(left, right);
        }
    }
}