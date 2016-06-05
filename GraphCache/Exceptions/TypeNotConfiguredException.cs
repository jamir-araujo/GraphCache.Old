using System;

namespace GraphCache.Exceptions
{
    internal class TypeNotMappedException : CacheException
    {
        public TypeNotMappedException(Type type)
            : base(string.Format("type {0} is not configured", type.FullName))
        {
        }
    }
}
