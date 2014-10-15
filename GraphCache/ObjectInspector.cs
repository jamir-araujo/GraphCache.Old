using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GraphCache
{
    internal class ObjectInspector
    {
        internal IEnumerable<Property> GetCacheableProperties(object value)
        {
            var cacheableProperties = new List<Property>();

            var type = value.GetType();
            var properties = type.GetProperties().Where(IsValidProperty);

            foreach (var property in properties)
            {
                cacheableProperties.Add(new Property
                {
                    Name = property.Name,
                    Value = property.GetValue(value, null)
                });
            }

            return cacheableProperties;
        }

        private bool IsValidProperty(PropertyInfo propertyInfo)
        {
            var notPrimitive = !propertyInfo.PropertyType.IsPrimitive;
            var notString = propertyInfo.PropertyType != typeof(string);
            var canReadAndWrite = propertyInfo.CanRead && propertyInfo.CanWrite;
            return (notString && notPrimitive && canReadAndWrite);
        }
    }
}
