using DillPickle.Framework.Exceptions;

namespace DillPickle.CommandLineRunner
{
    public class CommandLineRunnerException : DillPickleException
    {
        public CommandLineRunnerException(string message, params object[] objs) : base(message, objs)
        {
        }
    }
}