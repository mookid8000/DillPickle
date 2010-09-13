using System;
using System.IO;
using System.Linq;
using System.Text;
using DillPickle.Framework.Listeners;
using DillPickle.Framework.Parser.Api;
using DillPickle.Framework.Runner.Api;

namespace DillPickle.Framework.Runner
{
    public class DefaultCommandLineRunner : ICommandLineRunner
    {
        readonly IFeatureRunner featureRunner;
        readonly IGherkinParser gherkinParser;
        readonly IFeatureFileFinder featureFileFinder;
        readonly IActionStepsFinder actionStepsFinder;

        public DefaultCommandLineRunner(IActionStepsFinder actionStepsFinder, IFeatureRunner featureRunner, IFeatureFileFinder featureFileFinder, IGherkinParser gherkinParser)
        {
            this.actionStepsFinder = actionStepsFinder;
            this.featureRunner = featureRunner;
            this.featureFileFinder = featureFileFinder;
            this.gherkinParser = gherkinParser;
        }

        public void Execute(CommandLineArguments arguments)
        {
            var assemblyPath = arguments.AssemblyPath;
            var featurePattern = arguments.FeaturePattern;

            featureRunner.AddListener(new ConsoleWritingEventListener());

            var featuresToRun = featureFileFinder.Find(featurePattern)
                .SelectMany(fileName => gherkinParser.Parse(fileName, File.ReadAllText(fileName, Encoding.UTF8)).Features)
                .ToArray();

            Console.WriteLine("Found {0} features containing {1} executable scenarios", featuresToRun.Count(),
                              featuresToRun.Sum(f => f.Scenarios.Count));

            var actionStepsTypes = actionStepsFinder.FindTypesWithActionSteps(assemblyPath);

            featuresToRun.Select(f => featureRunner.Run(f, actionStepsTypes));
        }
    }
}