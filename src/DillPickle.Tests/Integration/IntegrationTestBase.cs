using System;
using System.Collections.Generic;
using System.Linq;
using DillPickle.Framework.Parser;
using DillPickle.Framework.Runner;
using DillPickle.Framework.Runner.Api;

namespace DillPickle.Tests.Integration
{
    public class IntegrationTestBase : FixtureBase
    {
        protected IList<FeatureResult> Run(string text, Type actionStepsType)
        {
            return Run(text, actionStepsType, DefaultOptions());
        }
        
        protected IList<FeatureResult> Run(string text, Type actionStepsType, RunnerOptions options)
        {
            var parser = new StupidGherkinParser();
            var result = parser.Parse(text);

            var activator = new TrivialObjectActivator();
            var runner = new FeatureRunner(activator,
                                           new IntelligentPropertySetter(new TrivialPropertySetter(), activator));

            return result.Features
                .Select(f => runner.Run(f, new[] {actionStepsType}, options))
                .ToList();
        }

        RunnerOptions DefaultOptions()
        {
            return new RunnerOptions
                       {
                           Filter = TagFilter.Empty(),
                           DryRun = false,
                       };
        }
    }
}