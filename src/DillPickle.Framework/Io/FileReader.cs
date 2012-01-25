using System.IO;
using System.Text;
using DillPickle.Framework.Io.Api;

namespace DillPickle.Framework.Io
{
    public class FileReader : IFileReader
    {
        public string Read(string fileName, Encoding encoding)
        {
            return File.ReadAllText(fileName, encoding);
        }
    }
}