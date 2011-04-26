using System;

namespace DillPickle.Framework.Exceptions
{
    ///<summary>
    /// Exception type to throw when a nice error message can be given,
    /// including all the sufficient pieces of context necessary to
    /// correct the error.
    ///</summary>
    public class FeatureExecutionException : DillPickleException
    {
        public FeatureExecutionException(string message, params object[] objs)
            : base(message, objs)
        {
        }

        public FeatureExecutionException(Exception innerException, string message, params object[] objs)
            : base(innerException, message, objs)
        {
        }
    }
}