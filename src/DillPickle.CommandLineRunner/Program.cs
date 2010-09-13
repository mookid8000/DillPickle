using DillPickle.Framework.Runner;
using DillPickle.Framework.Runner.Api;
using GoCommando;
using GoCommando.Api;
using GoCommando.Attributes;

namespace DillPickle.CommandLineRunner
{
    [Banner(@"DillPickle

(c) 2010 Mogens Heller Grabe
mookid8000@gmail.com
http://mookid.dk/oncode

Dill-flavored Gherkin-goodness for your BDD needs

Check out http://mookid.dk/oncode/dillpickle for more information.
")]
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

        [NamedArgument("dryrun", "d")]
        public bool DryRun { get; set; }

        static int Main(string[] args)
        {
            return Go.Run<Program>(args);
        }

        public void Run()
        {
            var runner = new DefaultCommandLineRunner();
            
            runner.Execute(new CommandLineArguments
                               {
                                   AssemblyPath = AssemblyPath,
                                   FeaturePattern = FeaturePattern,
                               });
        }
    }
}