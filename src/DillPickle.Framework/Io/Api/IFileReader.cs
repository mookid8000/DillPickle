using System.Text;

namespace DillPickle.Framework.Io.Api
{
    public interface IFileReader
    {
        string Read(string fileName, Encoding encoding);
    }
}