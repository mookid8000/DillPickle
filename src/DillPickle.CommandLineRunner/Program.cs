using System;
using System.IO;
using System.Linq;
using System.Reflection;
using DillPickle.Framework.Exceptions;
using DillPickle.Framework.Executor.Attributes;
using DillPickle.Framework.Parser;
using DillPickle.Framework.Runner;

namespace DillPickle.CommandLineRunner
{
    internal class Program
    {
        static int Main(string[] args)
        {
            ShowBanner();

            try
            {
                Run(args);

                return 0;
            }
            catch (DillPickleException e)
            {
                Console.WriteLine(e.Message);

                ShowHelpText();

                return -1;
            }
            catch(Exception e)
            {
                Console.WriteLine(e);

                return -1;
            }
        }

        static void ShowBanner()
        {
            Console.WriteLine(@"
Dill Pickle

(c) 2010 Mogens Heller Grabe
mookid8000@gmail.com
http://mookid.dk/oncode

Dill-flavored Gherkin-goodness for your BDD needs

");
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

        static void Run(string[] args)
        {
            if (args.Length != 2)
            {
                throw new CommandLineRunnerException("Please specify a path to an assembly on the command line");
            }

            var assemblyPath = args[0];
            var features = args[1];

            if (!File.Exists(assemblyPath))
            {
                throw new CommandLineRunnerException("Could not find assembly: {0}", assemblyPath);
            }

            if (!Path.IsPathRooted(features))
            {
                features = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, features);
            }

            var featureRunner = new FeatureRunner(new TrivialObjectActivator());
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