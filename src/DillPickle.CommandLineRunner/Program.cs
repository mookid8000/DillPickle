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
                Console.WriteLine(e.Message);

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

            var assemblyPath = args[0];
            var features = args[1];

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

            var assembly = Assembly.LoadFile(GenerateAbsolutePath(assemblyPath));

            var parser = new GherkinParser();
            var featureFiles = Directory.GetFiles(Path.GetDirectoryName(features), Path.GetFileName(features));
            var featuresToRun = featureFiles
                .SelectMany(fileName => parser.Parse(fileName, File.ReadAllText(fileName)).Features)
                .ToArray();

            var actionStepsTypes = assembly.GetTypes()
                .Where(HasActionStepsAttribute)
                .ToArray();

            Console.WriteLine("Found {0} features containing {1} executable scenarios", featuresToRun.Count(),
                              featuresToRun.Sum(f => f.Scenarios.Count));

            runner.Run(featuresToRun, actionStepsTypes);

            return;
        }

        static bool HasActionStepsAttribute(Type t)
        {
            return t.GetCustomAttributes(typeof (ActionStepsAttribute), false).Any();
        }

        static string GenerateAbsolutePath(string path)
        {
            return Path.IsPathRooted(path)
                       ? path
                       : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
        }
    }
}