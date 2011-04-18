using System;
using System.IO;

namespace DillPickle.Framework.Listeners
{
    ///<summary>
    /// Event listener that formats execution results and outputs them to a file.
    ///</summary>
    public class TextFileOutputEventListener : DefaultTextOutputEventListener
    {
        readonly string textOutputFile;

        public TextFileOutputEventListener(string textOutputFile)
        {
            this.textOutputFile = textOutputFile;
        }

        public override void Initialize()
        {
            base.Initialize();

            if (File.Exists(textOutputFile))
            {
                File.Delete(textOutputFile);
            }
        }

        protected override void WriteLineRaw(ConsoleColor color, int tabs, string text)
        {
            using (var reader = new StringReader(text))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    File.AppendAllText(textOutputFile, new string(' ', tabs*2) + line + Environment.NewLine);
                }
            }
        }

        protected override void WriteLineRaw()
        {
            File.AppendAllText(textOutputFile, Environment.NewLine);
        }
    }
}