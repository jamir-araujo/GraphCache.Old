using System;

namespace GraphCache.Exception
{
    internal class TypeNotMappedException : CacheException
    {
        public TypeNotMappedException(Type type)
            : base(string.Format("type {0} is not configured", type.FullName))
        {
        }
    }
}
