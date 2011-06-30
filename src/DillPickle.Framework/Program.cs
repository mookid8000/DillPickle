using System;
using DillPickle.Framework.Infrastructure;
using DillPickle.Framework.Io;
using DillPickle.Framework.Io.Api;
using DillPickle.Framework.Parser;
using DillPickle.Framework.Parser.Api;
using DillPickle.Framework.Runner;
using DillPickle.Framework.Runner.Api;
using GoCommando;
using GoCommando.Api;
using GoCommando.Attributes;

namespace DillPickle.Framework
{
    [Banner(@"DillPickle v. {assemblyVersion}

(c) 2010 Mogens Heller Grabe
mookid8000@gmail.com
http://mookid.dk/oncode

Dill-flavored Gherkin-goodness for your BDD needs

Check out http://mookid.dk/oncode/dillpickle for more information.

* All times displayed are in local time *")]
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

        [Description("Specifies that the runner should not actually execute any steps. Thus all results will be either PENDING or SUCCESS")]
        [NamedArgument("dryrun", "d")]
        public bool DryRun { get; set; }

        [NamedArgument("showcurrenttime", "t")]
        [Description("Specifies that all steps will have the current (local) time appended to the text output")]
        public bool ShowCurrentTime { get; set; }

        [NamedArgument("success", "s")]
        [Description("Specifies that the runner should stop executing if a step execution results in anything but success")]
        public bool SuccessRequired { get; set; }

        [NamedArgument("include", "i")]
        [Description("Specifies which tags to include")]
        public string Include { get; set; }

        [NamedArgument("exclude", "e")]
        [Description("Specifies which tags to exclude")]
        public string Exclude { get; set; }

        [NamedArgument("textoutput", "")]
        [Description("If set, the results of executing the features will be output to this file")]
        public string TextOutputFile { get; set; }

        static int Main(string[] args)
        {
            return Go.Run<Program>(args);
        }

        public void Run()
        {
            var container = new MiniContainer();

            container
                .MapType<IActionStepsFinder, ActionStepsFinder>()
                .MapType<IAssemblyLoader, AssemblyLoader>()
                .MapType<IFeatureRunner, FeatureRunner>()
                .MapType<IGherkinParser, StupidGherkinParser>()
                .MapType<IObjectActivator, TrivialObjectActivator>()
                .MapType<IPropertySetter, IntelligentPropertySetter>()
                .MapType<IPropertySetter, TrivialPropertySetter>()
                .MapType<IFeatureFileFinder, FeatureFileFinder>()
                .MapType<IFileReader, FileReader>();

            container.Configure<IntelligentPropertySetter>(s => s.AddAssembly(container.Resolve<IAssemblyLoader>().LoadConfiguredAssembly(AssemblyPath)));

            var runner = container.Resolve<DefaultCommandLineRunner>();

            runner.Execute(new CommandLineArguments
                               {
                                   AssemblyPath = AssemblyPath,
                                   FeaturePattern = FeaturePattern,
                                   TagsToInclude = Split(Include),
                                   TagsToExclude = Split(Exclude),
                                   DryRun = DryRun,
                                   SuccessRequired = SuccessRequired,
                                   ShowCurrentTime = ShowCurrentTime,
                                   TextOutputFile = TextOutputFile,
                               });
        }

        string[] Split(string text)
        {
            return (text ?? "").Split(",;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        }
    }
}