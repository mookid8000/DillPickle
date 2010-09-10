using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DillPickle.Framework.Executor.Attributes;
using DillPickle.Framework.Parser;
using DillPickle.Framework.Runner;
using GoCommando;
using GoCommando.Api;
using GoCommando.Attributes;

namespace DillPickle.CommandLineRunner
{
    [Banner(@"DillPickle

(c) 2010 Mogens Heller Grabe
mookid8000@gmail.com
http://mookid.dk/oncode

Dill-flavored Gherkin-goodness for your BDD needs

Check out http://mookid.dk/oncode/dillpickle for more information.
")]
    public class Program : ICommando
    {
        [PositionalArgument]
        [Description("Path to the assembly containing classes with [ActionSteps] and [TypeConverter]s")]
        [Example(@"..\src\SomeProject.Specs\bin\Debug\SomeProject.Specs.dll")]
        public string AssemblyPath { get; set; }

        [PositionalArgument]
        [Description("File pattern of feature files to load")]
        [Example(@"..\src\SomeProject.Specs\Features\*.feature")]
        public string FeaturePattern { get; set; }

        [NamedArgument("dryrun", "d")]
        public bool DryRun { get; set; }

        static int Main(string[] args)
        {
            return Go.Run<Program>(args);
        }

        public void Run()
        {
            if (!File.Exists(AssemblyPath))
            {
                throw new CommandLineRunnerException("Could not find assembly: {0}", AssemblyPath);
            }

            if (!Path.IsPathRooted(FeaturePattern))
            {
                FeaturePattern = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FeaturePattern);
            }

            var assembly = Assembly.LoadFrom(GenerateAbsolutePath(AssemblyPath));

            var configPath = GenerateAbsolutePath(AssemblyPath) + ".config";
            if (File.Exists(configPath))
            {
                AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", configPath);
            }

            var objectActivator = new TrivialObjectActivator();
            var featureRunner = new FeatureRunner(objectActivator, new IntelligentPropertySetter(new TrivialPropertySetter(), assembly, objectActivator));
            featureRunner.AddListener(new ConsoleWritingEventListener());

            var runner = new DefaultRunner(featureRunner);

            var parser = new GherkinParser();
            var featureFiles = Directory.GetFiles(Path.GetDirectoryName(FeaturePattern), Path.GetFileName(FeaturePattern));
            var featuresToRun = featureFiles
                .SelectMany(fileName => parser.Parse(fileName, File.ReadAllText(fileName, Encoding.UTF8)).Features)
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