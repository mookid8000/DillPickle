using System;

namespace DillPickle.Framework.Runner.Api
{
    ///<summary>
    /// Specified the arguments for the command line runner to use
    ///</summary>
    public class CommandLineArguments
    {
        ///<summary>
        /// Path (absolute or relative) to the assembly from which to load action steps
        ///</summary>
        
        public string AssemblyPath { get; set; }
        ///<summary>
        /// Path or pattern (absolute or relative) to feature file to run
        ///</summary>
        public string FeaturePattern { get; set; }
        
        ///<summary>
        /// Optionally specifies which tags to include (thus executing ONLY features/scenarios tagged with that)
        ///</summary>
        public string[] TagsToInclude { get; set; }
        
        ///<summary>
        /// Optionally specified which tags to exclude (thus executing everything EXCEPT features/scenarios tagged with that)
        ///</summary>
        public string[] TagsToExclude { get; set; }

        ///<summary>
        /// Indicates whether the runner should only simulate running the action steps
        ///</summary>
        public bool DruRun { get; set; }

        /// <summary>
        /// Indicates whether the runner should stop execution upon encountering an error
        /// </summary>
        public bool SuccessRequired { get; set; }

        /// <summary>
        /// Indicates that the console output should have the current time appended to the text of each step.
        /// </summary>
        public bool ShowTimestamps { get; set; }
    }
}