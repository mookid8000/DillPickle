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
            featureFileFinder.Stub(f => f.Find("weird pattern")).Return(new[] {"file1", "file2"});

            fileReader.Stub(r => r.Read("file1", Encoding.UTF8)).Return("bla1");
            fileReader.Stub(r => r.Read("file2", Encoding.UTF8)).Return("bla2");

            var feature1 = new Feature("feature1", new string[0]);
            var feature2 = new Feature("feature2", new string[0]);
            
            gherkinParser.Stub(p => p.Parse("file1", "bla1")).Return(new ParseResult(new List<Feature>{feature1}));
            gherkinParser.Stub(p => p.Parse("file2", "bla2")).Return(new ParseResult(new List<Feature>{feature2}));

            var actionStepsTypes = new Type[0];
            actionStepsFinder.Stub(a => a.FindTypesWithActionSteps("some path")).Return(actionStepsTypes);

            sut.Execute(new CommandLineArguments
                            {
                                AssemblyPath = "some path",
                                FeaturePattern = "weird pattern"
                            });

            featureRunner.AssertWasCalled(r => r.Run(feature1, actionStepsTypes));
            featureRunner.AssertWasCalled(r => r.Run(feature2, actionStepsTypes));
        }
    }
}