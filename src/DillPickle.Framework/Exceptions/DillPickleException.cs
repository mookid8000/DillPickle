using System;

namespace DillPickle.Framework.Exceptions
{
    public abstract class DillPickleException : Exception
    {
        protected DillPickleException(string message, params object[] objs)
            : base(string.Format(message, objs))
        {
        }
    }
}