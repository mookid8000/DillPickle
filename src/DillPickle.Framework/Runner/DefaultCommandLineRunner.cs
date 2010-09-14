using System;
using System.Linq;
using System.Text;
using DillPickle.Framework.Extensions;
using DillPickle.Framework.Io.Api;
using DillPickle.Framework.Listeners;
using DillPickle.Framework.Parser.Api;
using DillPickle.Framework.Runner.Api;

namespace DillPickle.Framework.Runner
{
    public class DefaultCommandLineRunner : ICommandLineRunner
    {
        readonly IFeatureRunner featureRunner;
        readonly IGherkinParser gherkinParser;
        readonly IFileReader fileReader;
        readonly IFeatureFileFinder featureFileFinder;
        readonly IActionStepsFinder actionStepsFinder;

        public DefaultCommandLineRunner(IActionStepsFinder actionStepsFinder, IFeatureRunner featureRunner, IFeatureFileFinder featureFileFinder, IGherkinParser gherkinParser, IFileReader fileReader)
        {
            this.actionStepsFinder = actionStepsFinder;
            this.featureRunner = featureRunner;
            this.featureFileFinder = featureFileFinder;
            this.gherkinParser = gherkinParser;
            this.fileReader = fileReader;
        }

        public void Execute(CommandLineArguments arguments)
        {
            var assemblyPath = arguments.AssemblyPath;
            var featurePattern = arguments.FeaturePattern;

            featureRunner.AddListener(new ConsoleWritingEventListener());

            var featuresToRun = featureFileFinder.Find(featurePattern)
                .SelectMany(fileName => gherkinParser.Parse(fileName, fileReader.Read(fileName, Encoding.UTF8)).Features);

            Console.WriteLine("Found {0} features containing {1} executable scenarios", featuresToRun.Count(),
                              featuresToRun.Sum(f => f.Scenarios.Count));

            var actionStepsTypes = actionStepsFinder.FindTypesWithActionSteps(assemblyPath);

            featuresToRun.ForEach(f => featureRunner.Run(f, actionStepsTypes));
        }
    }
}