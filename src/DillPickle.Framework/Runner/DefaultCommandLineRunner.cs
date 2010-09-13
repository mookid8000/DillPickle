using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DillPickle.Framework.Exceptions;
using DillPickle.Framework.Executor.Attributes;
using DillPickle.Framework.Listeners;
using DillPickle.Framework.Parser;
using DillPickle.Framework.Runner.Api;

namespace DillPickle.Framework.Runner
{
    public class DefaultCommandLineRunner
    {
        public void Execute(CommandLineArguments arguments)
        {
            var assemblyPath = arguments.AssemblyPath;
            var featurePattern = arguments.FeaturePattern;

            if (!File.Exists(assemblyPath))
            {
                throw new CommandLineRunnerException("Could not find assembly: {0}", assemblyPath);
            }

            if (!Path.IsPathRooted(featurePattern))
            {
                featurePattern = Path.Combine(Environment.CurrentDirectory, featurePattern);
            }

            var assembly = Assembly.LoadFrom(GenerateAbsolutePath(assemblyPath));

            var configPath = GenerateAbsolutePath(assemblyPath) + ".config";
            if (File.Exists(configPath))
            {
                AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", configPath);
            }

            var objectActivator = new TrivialObjectActivator();
            var featureRunner = new FeatureRunner(objectActivator, new IntelligentPropertySetter(new TrivialPropertySetter(), assembly, objectActivator));
            featureRunner.AddListener(new ConsoleWritingEventListener());

            var runner = new DefaultRunner(featureRunner);

            var parser = new GherkinParser();
            var featureFiles = Directory.GetFiles(Path.GetDirectoryName(featurePattern), Path.GetFileName(featurePattern));
            var featuresToRun = featureFiles
                .SelectMany(fileName => parser.Parse(fileName, File.ReadAllText(fileName, Encoding.UTF8)).Features)
                .ToArray();

            var actionStepsTypes = assembly.GetTypes()
                .Where(HasActionStepsAttribute)
                .ToArray();

            Console.WriteLine("Found {0} features containing {1} executable scenarios", featuresToRun.Count(),
                              featuresToRun.Sum(f => f.Scenarios.Count));

            runner.Run(featuresToRun, actionStepsTypes);
        }

        bool HasActionStepsAttribute(Type t)
        {
            return t.GetCustomAttributes(typeof(ActionStepsAttribute), false).Any();
        }

        string GenerateAbsolutePath(string path)
        {
            return Path.IsPathRooted(path)
                       ? path
                       : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
        }
    }
}