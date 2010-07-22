namespace DillPickle.Framework.Exceptions
{
    public class FeatureExecutionException : DillPickleException
    {
        public FeatureExecutionException(string message, params object[] objs)
            : base(message, objs)
        {
        }
    }
}