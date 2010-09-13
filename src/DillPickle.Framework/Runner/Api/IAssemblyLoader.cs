using System.Reflection;

namespace DillPickle.Framework.Runner.Api
{
    public interface IAssemblyLoader
    {
        Assembly LoadAssemblyWithApplicationConfigurationIfPossible(string assemblyPath);
    }
}