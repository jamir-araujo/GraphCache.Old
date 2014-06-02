using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GraphCache
{
    internal class ObjectInspector
    {
        private readonly CacheConfiguration _configuration;

        internal ObjectInspector(CacheConfiguration configuration)
        {
            _configuration = configuration;
        }

        internal IEnumerable<Property> GetCacheableProperties(object value)
        {
            var cacheableProperties = new List<Property>();

            var type = value.GetType().GetTypeInfo();
            var properties = type.GetProperties().Where(IsConfigured);

            foreach (var property in properties)
            {
                cacheableProperties.Add(new Property
                {
                    Name = property.Name,
                    Value = property.GetValue(value)
                });
            }

            return cacheableProperties;
        }

        private bool IsConfigured(PropertyInfo propertyInfo)
        {
            var typeInfo = propertyInfo.PropertyType.GetTypeInfo();
            var IsIEnumerable = typeInfo.ImplementedInterfaces.Contains(typeof(IEnumerable));
            var notPrimitive = !typeInfo.IsPrimitive;
            var notString = propertyInfo.PropertyType != typeof(string);
            return (notString && notPrimitive && IsIEnumerable) || _configuration.Contains(propertyInfo.PropertyType);
        }
    }
}
