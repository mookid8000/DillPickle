using System;
using System.Collections.Generic;
using System.Linq;

namespace DillPickle.Framework.Parser.Api
{
    public class Step : IEquatable<Step>
    {
        Step(string text, string prefix)
        {
            Parameters = new List<Dictionary<string, string>>();
            Text = text;
            Prefix = prefix;
        }

        public string Text { get; set; }
        public string Prefix { get; set; }
        public StepType StepType { get; set; }
        public List<Dictionary<string, string>> Parameters { get; set; }

        public static Step And(string text, StepType previousStepType)
        {
            return new Step(text, "And") {StepType = previousStepType};
        }

        public static Step Given(string text)
        {
            return new Step(text, "Given") {StepType = StepType.Given};
        }

        public static Step When(string text)
        {
            return new Step(text, "When") {StepType = StepType.When};
        }

        public static Step Then(string text)
        {
            return new Step(text, "Then") {StepType = StepType.Then};
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", StepType, Text);
        }

        public Step SubstituteAndClone(Dictionary<string, string> dictionary)
        {
            return new Step(Substitute(dictionary), Prefix)
                       {
                           StepType = StepType,
                           Parameters = Parameters
                       };
        }

        string Substitute(Dictionary<string, string> dictionary)
        {
            var text = Text;

            foreach(var kvp in dictionary)
            {
                text = text.Replace(string.Format("<{0}>", kvp.Key), kvp.Value);
            }

            return text;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (Step)) return false;
            return Equals((Step) obj);
        }

        public bool Equals(Step other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            
            return Equals(other.Text, Text)
                && Equals(other.Prefix, Prefix) 
                && Equals(other.StepType, StepType)
                && DictionaryListsAreEqual(Parameters, other.Parameters);
        }

        bool DictionaryListsAreEqual(List<Dictionary<string, string>> list1, List<Dictionary<string, string>> list2)
        {
            if (list1.Count != list2.Count) return false;

            foreach (var index in Enumerable.Range(0, list1.Count))
            {
                var dict1 = list1[index];
                var dict2 = list2[index];

                if (dict1.Count != dict2.Count) return false;

                if (dict1.Any(kvp => !dict2.ContainsKey(kvp.Key) || dict2[kvp.Key] != kvp.Value))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Text != null ? Text.GetHashCode() : 0);
                result = (result*397) ^ (Prefix != null ? Prefix.GetHashCode() : 0);
                result = (result*397) ^ StepType.GetHashCode();
                return result;
            }
        }

        public static bool operator ==(Step left, Step right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Step left, Step right)
        {
            return !Equals(left, right);
        }

        public bool Matches(Step stepToMatch)
        {
            return stepToMatch == this;
        }
    }
}