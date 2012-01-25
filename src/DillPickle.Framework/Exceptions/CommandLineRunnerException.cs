namespace DillPickle.Framework.Exceptions
{
    public class CommandLineRunnerException : DillPickleException
    {
        public CommandLineRunnerException(string message, params object[] objs) : base(message, objs)
        {
        }
    }
}