using System;

namespace DillPickle.Framework.Executor.Attributes
{
    /// <summary>
    /// Indicates that the class contains step methods that can potentially
    /// be executed by DillPickle.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ActionStepsAttribute : Attribute
    {
    }
}