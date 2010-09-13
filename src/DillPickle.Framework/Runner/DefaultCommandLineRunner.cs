using System;
using System.IO;
using System.Linq;
using System.Text;
using DillPickle.Framework.Listeners;
using DillPickle.Framework.Parser;
using DillPickle.Framework.Parser.Api;
using DillPickle.Framework.Runner.Api;

namespace DillPickle.Framework.Runner
{
    public class DefaultCommandLineRunner : ICommandLineRunner
    {
        readonly IFeatureRunner featureRunner;
        readonly IRunner runner;
        readonly IGherkinParser parser;
        readonly IFeatureFileFinder finder;
        readonly IActionStepsFinder actionStepsFinder;

        public DefaultCommandLineRunner()
        {
            var objectActivator = new TrivialObjectActivator();
            var propertySetter = new IntelligentPropertySetter(new TrivialPropertySetter(), objectActivator);
            var assemblyLoader = new AssemblyLoader();
            actionStepsFinder = new ActionStepsFinder(assemblyLoader);
            featureRunner = new FeatureRunner(objectActivator, propertySetter);
            runner = new DefaultRunner(featureRunner);
            parser = new GherkinParser();
            finder = new FeatureFileFinder();
        }

        public void Execute(CommandLineArguments arguments)
        {
            var assemblyPath = arguments.AssemblyPath;
            var featurePattern = arguments.FeaturePattern;

            featureRunner.AddListener(new ConsoleWritingEventListener());

            var featuresToRun = finder.Find(featurePattern)
                .SelectMany(fileName => parser.Parse(fileName, File.ReadAllText(fileName, Encoding.UTF8)).Features)
                .ToArray();

            Console.WriteLine("Found {0} features containing {1} executable scenarios", featuresToRun.Count(),
                              featuresToRun.Sum(f => f.Scenarios.Count));

            var actionStepsTypes = actionStepsFinder.FindTypesWithActionSteps(assemblyPath);

            runner.Run(featuresToRun, actionStepsTypes);
        }
    }
}