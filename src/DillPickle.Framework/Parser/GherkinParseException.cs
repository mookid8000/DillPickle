using System;
using System.IO;

namespace DillPickle.Framework.Parser
{
    public class GherkinParseException : Exception
    {
        public GherkinParseException(string fileName, int lineNumber, string line, string message, params object[] objs)
            : base(string.Format(@"Error parsing file {0} line {1}: {2}

{3}",
                                 Path.GetFileName(fileName),
                                 lineNumber,
                                 line,
                                 string.Format(message, objs)))
        {
        }
    }
}