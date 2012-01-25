using System;
using System.IO;
using System.Reflection;
using DillPickle.Framework.Runner.Api;

namespace DillPickle.Framework.Runner
{
    public class AssemblyLoader : IAssemblyLoader
    {
        public Assembly LoadConfiguredAssembly(string assemblyPath)
        {
            var absoluteAssemblyPath = GenerateAbsolutePath(assemblyPath);

            var assembly = LoadAssembly(assemblyPath);

            var configPath = string.Format("{0}.config", absoluteAssemblyPath);

            if (File.Exists(configPath))
            {
                AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", configPath);
            }

            return assembly;
        }

        Assembly LoadAssembly(string assemblyPath)
        {
            var absoluteAssemblyPath = GenerateAbsolutePath(assemblyPath);
            
            AssertFileExists(absoluteAssemblyPath, "Assembly {0} does not exist", assemblyPath);

            return Assembly.LoadFrom(absoluteAssemblyPath);
        }

        string GenerateAbsolutePath(string path)
        {
            return Path.IsPathRooted(path)
                       ? path
                       : Path.Combine(Environment.CurrentDirectory, path);
        }

        void AssertFileExists(string path, string errorMessage, params object[] objs)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(string.Format(errorMessage, objs));
            }
        }
    }
}