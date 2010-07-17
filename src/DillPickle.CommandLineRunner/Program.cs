using System;
using System.IO;
using System.Linq;
using System.Reflection;
using DillPickle.Framework.Executor.Attributes;
using DillPickle.Framework.Parser;
using DillPickle.Framework.Runner;

namespace DillPickle.CommandLineRunner
{
    internal class Program
    {
        static int Main(string[] args)
        {
            try
            {
                Run(args);
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 1;
            }
        }

        static void Run(string[] args)
        {
            if (args.Length != 2)
            {
                throw new InvalidOperationException(
                    string.Format("Please specify a path to an assembly on the command line"));
            }

            string assemblyPath = args[0];
            string features = args[1];

            if (!File.Exists(assemblyPath))
            {
                throw new InvalidOperationException(string.Format("Could not find assembly: {0}", assemblyPath));
            }

            if (!Path.IsPathRooted(features))
            {
                features = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, features);
            }

            var featureRunner = new FeatureRunner();
            featureRunner.AddListener(new ConsoleWritingEventListener());

            var runner = new Runner(featureRunner);

            Assembly assembly = Assembly.LoadFile(GenerateAbsolutePath(assemblyPath));

            runner.Run(Directory.GetFiles(Path.GetDirectoryName(features), Path.GetFileName(features))
                           .SelectMany(
                           fileName => new GherkinParser().Parse(fileName, File.ReadAllText(fileName)).Features)
                           .ToArray(),
                       assembly.GetTypes().Where(t => t.GetCustomAttributes(typeof (ActionStepsAttribute), false).Any())
                           .ToArray());

            return;
        }

        static string GenerateAbsolutePath(string path)
        {
            return Path.IsPathRooted(path)
                       ? path
                       : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
        }
    }
}