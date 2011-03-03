namespace DillPickle.Framework.Runner
{
    public class RunnerOptions
    {
        public RunnerOptions()
        {
            Filter = TagFilter.Empty();
        }

        public TagFilter Filter { get; set; }
        public bool DruRun { get; set; }

        public bool SuccessRequired { get; set; }

        public bool Equals(RunnerOptions other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Filter, Filter) && other.DruRun.Equals(DruRun);
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
                return ((Filter != null ? Filter.GetHashCode() : 0)*397) ^ DruRun.GetHashCode();
            }
        }
    }
}