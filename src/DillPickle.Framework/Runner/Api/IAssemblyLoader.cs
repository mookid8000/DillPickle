using System.Reflection;

namespace DillPickle.Framework.Runner.Api
{
    ///<summary>
    /// Service responsible of loading the specified assembly.
    ///</summary>
    public interface IAssemblyLoader
    {
        ///<summary>
        /// Loads .NET assembly with the specified path, possibly
        /// including the corresponding app.config if one exists.
        ///</summary>
        Assembly LoadConfiguredAssembly(string assemblyPath);
    }
}