using System;
using System.IO;
using System.Reflection;
using DillPickle.Framework.Runner.Api;

namespace DillPickle.Framework.Runner
{
    public class AssemblyLoader : IAssemblyLoader
    {
        public Assembly LoadAssemblyWithApplicationConfigurationIfPossible(string assemblyPath)
        {
            var absoluteAssemblyPath = GenerateAbsolutePath(assemblyPath);
            AssertFileExists(absoluteAssemblyPath, "Assembly {0} does not exist", assemblyPath);

            var assembly = Assembly.LoadFrom(absoluteAssemblyPath);

            var configPath = string.Format("{0}.config", absoluteAssemblyPath);

            if (File.Exists(configPath))
            {
                AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", configPath);
            }

            return assembly;
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