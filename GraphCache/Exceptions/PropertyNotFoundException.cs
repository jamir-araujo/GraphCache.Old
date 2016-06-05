using System;

namespace GraphCache.Exceptions
{
    internal class PropertyNotFoundException : CacheException
    {
        public PropertyNotFoundException(Type type, string propertyName)
            : base(string.Format("Property {0} not found in the type {1}", propertyName, type.FullName))
        { }
    }
}
