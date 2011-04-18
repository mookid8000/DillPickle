using System;
using System.Linq;
using System.Text;
using DillPickle.Framework.Io.Api;
using DillPickle.Framework.Listeners;
using DillPickle.Framework.Parser.Api;
using DillPickle.Framework.Runner.Api;

namespace DillPickle.Framework.Runner
{
    ///<summary>
    /// Command line runner that runs features and stuff, without too many surprises
    ///</summary>
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

            var filter = new TagFilter(arguments.TagsToInclude, arguments.TagsToExclude);

            var featuresToRun = featureFileFinder.Find(featurePattern)
                .SelectMany(fileName => gherkinParser.Parse(fileName, fileReader.Read(fileName, Encoding.UTF8)).Features)
                .Where(f => filter.IsSatisfiedBy(f.Tags));

            Console.WriteLine("Found {0} features containing {1} executable scenarios", featuresToRun.Count(),
                              featuresToRun.Sum(f => f.Scenarios.Count));

            var actionStepsTypes = actionStepsFinder.FindTypesWithActionSteps(assemblyPath);

            var options = new RunnerOptions
                              {
                                  Filter = filter,
                                  DruRun = arguments.DruRun,
                                  SuccessRequired = arguments.SuccessRequired,
                              };

            SetUpListeners(arguments);

            featureRunner.Commission();

            foreach(var feature in featuresToRun)
            {
                var featureResult = featureRunner.Run(feature, actionStepsTypes, options);

                if (options.SuccessRequired && !featureResult.Success) break;
            }

            featureRunner.Decommission();
        }

        void SetUpListeners(CommandLineArguments arguments)
        {
            featureRunner.AddListener(new ConsoleWritingEventListener
                                          {
                                              ShowCurrentTimes = arguments.ShowCurrentTime,
                                          });

            if (arguments.TextOutputFile.IsSet())
            {
                Console.WriteLine("Outputting results to {0}", arguments.TextOutputFile);

                featureRunner.AddListener(new TextFileOutputEventListener(arguments.TextOutputFile));
            }
        }
    }
}