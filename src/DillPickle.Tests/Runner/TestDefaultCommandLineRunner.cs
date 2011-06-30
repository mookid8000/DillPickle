using System;
using System.Collections.Generic;
using System.Text;
using DillPickle.Framework.Io.Api;
using DillPickle.Framework.Parser.Api;
using DillPickle.Framework.Runner;
using DillPickle.Framework.Runner.Api;
using NUnit.Framework;
using Rhino.Mocks;

namespace DillPickle.Tests.Runner
{
    [TestFixture]
    public class TestDefaultCommandLineRunner : FixtureBase
    {
        DefaultCommandLineRunner sut;
        IActionStepsFinder actionStepsFinder;
        IFeatureRunner featureRunner;
        IFeatureFileFinder featureFileFinder;
        IGherkinParser gherkinParser;
        IFileReader fileReader;

        public override void DoSetUp()
        {
            actionStepsFinder = Mock<IActionStepsFinder>();
            featureRunner = Mock<IFeatureRunner>();
            featureFileFinder = Mock<IFeatureFileFinder>();
            gherkinParser = Mock<IGherkinParser>();
            fileReader = Mock<IFileReader>();
            sut = new DefaultCommandLineRunner(actionStepsFinder, featureRunner, featureFileFinder, gherkinParser, fileReader);
        }

        [Test]
        public void DoesItsThing()
        {
            featureFileFinder.Stub(f => f.Find("weird pattern")).Return(new[] {"file1", "file2", "file3"});

            var feature1 = Stub("file1", "bla1", "feature1", new[] { "bom" });
            var feature2 = Stub("file2", "bla2", "feature2", new[] { "bom" });
            var feature3 = Stub("file3", "bla3", "feature3", new[] { "bim" });

            var actionStepsTypes1 = new Type[0];
            var actionStepsTypes2 = new Type[0];
            var actionStepsTypes3 = new Type[0];
            actionStepsFinder.Stub(a => a.FindTypesWithActionSteps("some path", "file1")).Return(actionStepsTypes1);
            actionStepsFinder.Stub(a => a.FindTypesWithActionSteps("some path", "file2")).Return(actionStepsTypes2);
            actionStepsFinder.Stub(a => a.FindTypesWithActionSteps("some path", "file3")).Return(actionStepsTypes3);

            sut.Execute(new CommandLineArguments
                            {
                                AssemblyPath = "some path",
                                FeaturePattern = "weird pattern",
                                TagsToExclude = new[] {"bim"},
                                TagsToInclude = new[] {"bom"},
                            });

            var expectedOptions = new RunnerOptions
                                     {
                                         Filter = new TagFilter(new[] {"bom"}, new[] {"bim"}),
                                         DryRun = false,
                                     };

            featureRunner.AssertWasCalled(r => r.Run(feature1, actionStepsTypes1, expectedOptions));
            featureRunner.AssertWasCalled(r => r.Run(feature2, actionStepsTypes2, expectedOptions));
            featureRunner.AssertWasNotCalled(r => r.Run(feature3, actionStepsTypes3, expectedOptions));
        }

        [Test]
        public void StopsExecutingIfErrorIsEncountered()
        {
            featureFileFinder.Stub(f => f.Find(Arg<string>.Is.Anything)).Return(new[] { "file1", "file2" });
            actionStepsFinder.Stub(a => a.FindTypesWithActionSteps(Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return(new Type[0]);

            var feature1 = Stub("file1", "bla1", "feature1", new[] { "bom" });
            var feature2 = Stub("file2", "bla2", "feature2", new[] { "bom" });

            var expectedOptions = new RunnerOptions
                                      {
                                          Filter = TagFilter.Empty(),
                                          DryRun = false,
                                          SuccessRequired = true,
                                      };

            featureRunner.Stub(r => r.Run(feature1, new Type[0], expectedOptions))
                .Return(new FeatureResult(feature1)
                            {
                                ScenarioResults =
                                    {
                                        new ScenarioResult("has an error")
                                            {
                                                StepResults = {new StepResult("is an error") {Result = Result.Failed}}
                                            }
                                    }
                            });

            sut.Execute(new CommandLineArguments
                            {
                                AssemblyPath = "some path",
                                FeaturePattern = "weird pattern",
                                TagsToExclude = new string[0],
                                TagsToInclude = new string[0],
                                SuccessRequired = true,
                            });

            featureRunner.AssertWasCalled(r => r.Run(feature1, new Type[0], expectedOptions));
            featureRunner.AssertWasNotCalled(r => r.Run(feature2, new Type[0], expectedOptions));
        }

        [Test]
        public void StopsExecutingIfPendingIsEncountered()
        {
            featureFileFinder.Stub(f => f.Find(Arg<string>.Is.Anything)).Return(new[] { "file1", "file2" });
            actionStepsFinder.Stub(a => a.FindTypesWithActionSteps(Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return(new Type[0]);

            var feature1 = Stub("file1", "bla1", "feature1", new[] { "bom" });
            var feature2 = Stub("file2", "bla2", "feature2", new[] { "bom" });

            var expectedOptions = new RunnerOptions
                                      {
                                          Filter = TagFilter.Empty(),
                                          DryRun = false,
                                          SuccessRequired = true,
                                      };

            featureRunner.Stub(r => r.Run(feature1, new Type[0], expectedOptions))
                .Return(new FeatureResult(feature1)
                            {
                                ScenarioResults =
                                    {
                                        new ScenarioResult("has an error")
                                            {
                                                StepResults = {new StepResult("is an error") {Result = Result.Pending}}
                                            }
                                    }
                            });

            sut.Execute(new CommandLineArguments
                            {
                                AssemblyPath = "some path",
                                FeaturePattern = "weird pattern",
                                TagsToExclude = new string[0],
                                TagsToInclude = new string[0],
                                SuccessRequired = true,
                            });

            featureRunner.AssertWasCalled(r => r.Run(feature1, new Type[0], expectedOptions));
            featureRunner.AssertWasNotCalled(r => r.Run(feature2, new Type[0], expectedOptions));
        }

        Feature Stub(string fileName, string gherkinText, string featureName, string[] featureTags)
        {
            fileReader.Stub(r => r.Read(fileName, Encoding.UTF8)).Return(gherkinText);

            var feature = new Feature(featureName, featureTags);
            
            gherkinParser.Stub(p => p.Parse(fileName, gherkinText)).Return(new ParseResult(new List<Feature> { feature }));
            
            return feature;
        }
    }
}