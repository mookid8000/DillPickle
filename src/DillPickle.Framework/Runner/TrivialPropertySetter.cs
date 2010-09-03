using System;
using System.Reflection;
using DillPickle.Framework.Runner.Api;

namespace DillPickle.Framework.Runner
{
    public class TrivialPropertySetter : IPropertySetter
    {
        public void SetValue(object instance, PropertyInfo property, string value)
        {
            property.SetValue(instance, Convert.ChangeType(value, property.PropertyType), null);
        }
    }
}