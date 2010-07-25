using System.IO;

namespace DillPickle.Framework.Exceptions
{
    public class GherkinParseException : DillPickleException
    {
        public GherkinParseException(string fileName, int lineNumber, string line, string message, params object[] objs)
            : base(@"Error parsing file {0} line {1}: {2}

{3}",
                                 Path.GetFileName(fileName),
                                 lineNumber,
                                 line,
                                 string.Format(message, objs))
        {
        }

        public GherkinParseException(string fileName, string message, params object[] objs)
            : base(@"Error parsing file {0}

{1}", fileName, string.Format(message, objs))
        {

        }
    }
}