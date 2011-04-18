using System;
using System.IO;

namespace DillPickle.Framework.Listeners
{
    ///<summary>
    /// Event listener that formats execution results and outputs them to the console.
    ///</summary>
    public class ConsoleWritingEventListener : DefaultTextOutputEventListener
    {
        protected override void WriteLineRaw(ConsoleColor color, int tabs, string text)
        {
            using (new ConsoleColorContext(color))
            using (var reader = new StringReader(text))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    Console.WriteLine(new string(' ', tabs * 2) + line);
                }
            }
        }

        protected override void WriteLineRaw()
        {
            Console.WriteLine();
        }

        class ConsoleColorContext : IDisposable
        {
            readonly ConsoleColor previousColor;

            public ConsoleColorContext(ConsoleColor newColor)
            {
                previousColor = Console.ForegroundColor;
                Console.ForegroundColor = newColor;
            }

            public void Dispose()
            {
                Console.ForegroundColor = previousColor;
            }
        }
    }
}