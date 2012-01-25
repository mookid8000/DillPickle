namespace DillPickle.Framework.Runner.Api
{
    public interface ICommandLineRunner
    {
        void Execute(CommandLineArguments arguments);
    }
}