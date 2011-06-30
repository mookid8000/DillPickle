using System;

namespace DillPickle.Framework.Runner
{
    public class RunnerOptions : IEquatable<RunnerOptions>
    {
        public RunnerOptions()
        {
            Filter = TagFilter.Empty();
        }

        public TagFilter Filter { get; set; }
        public bool DryRun { get; set; }
        public bool SuccessRequired { get; set; }

        public bool Equals(RunnerOptions other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Filter, Filter) && other.DryRun.Equals(DryRun) && other.SuccessRequired.Equals(SuccessRequired);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (RunnerOptions)) return false;
            return Equals((RunnerOptions) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Filter != null ? Filter.GetHashCode() : 0);
                result = (result*397) ^ DryRun.GetHashCode();
                result = (result*397) ^ SuccessRequired.GetHashCode();
                return result;
            }
        }

        public static bool operator ==(RunnerOptions left, RunnerOptions right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(RunnerOptions left, RunnerOptions right)
        {
            return !Equals(left, right);
        }
    }
}