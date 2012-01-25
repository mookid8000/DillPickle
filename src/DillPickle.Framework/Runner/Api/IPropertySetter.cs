using System.Reflection;

namespace DillPickle.Framework.Runner.Api
{
    public interface IPropertySetter
    {
        void SetValue(object instance, PropertyInfo property, string value);
    }
}