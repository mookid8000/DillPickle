using System;
using System.IO;
using System.Linq;
using System.Reflection;
using DillPickle.Framework.Exceptions;
using DillPickle.Framework.Executor.Attributes;
using DillPickle.Framework.Parser;
using DillPickle.Framework.Runner;
using GoCommando;

namespace DillPickle.CommandLineRunner
{
    [Banner(@"DillPickle

(c) 2010 Mogens Heller Grabe
mookid8000@gmail.com
http://mookid.dk/oncode

Dill-flavored Gherkin-goodness for your BDD needs
")]
    public class Program : IGoCommando
    {
        [PositionalArgument(0)]
        public string AssemblyPath { get; set; }

        [PositionalArgument(1)]
        public string FeaturePattern { get; set; }

        [NamedArgument("dryrun", "d")]
        public bool DryRun { get; set; }

        static int Main(string[] args)
        {
            try
            {
                return Go.Run<Program>(args);
            }
            catch (DillPickleException e)
            {
                Console.WriteLine(e.Message);

                ShowHelpText();

                return 1;
            }
            catch (ReflectionTypeLoadException e)
            {
                var loaderExceptions = string.Join(Environment.NewLine, e.LoaderExceptions.Select(ex => ex.ToString()).ToArray());

                Console.WriteLine(@"{0}

Loader exceptions:

{1}", e, loaderExceptions);

                return 1;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                return 2;
            }
        }

        static void ShowHelpText()
        {
            Console.WriteLine(@"
Usage:

    dill.exe <assembly-path> <feature-pattern>

where <assembly-path> is a relative or absolute path to an assembly containing
classes with action steps, and <feature-pattern> is a relative or absolute path
(possiblt containing wildcars) to the location of your feature files.

E.g.:

    dill ..\src\MyProj\bin\Debug\Asm.dll ..\src\MyProj\Features\03*.feature
");
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

            var featureRunner = new FeatureRunner(new TrivialObjectActivator());
            featureRunner.AddListener(new ConsoleWritingEventListener());

            var runner = new DefaultRunner(featureRunner);

            var assembly = Assembly.LoadFrom(GenerateAbsolutePath(AssemblyPath));

            var parser = new GherkinParser();
            var featureFiles = Directory.GetFiles(Path.GetDirectoryName(FeaturePattern), Path.GetFileName(FeaturePattern));
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