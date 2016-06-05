using System;

namespace GraphCache.Exceptions
{
    internal class TypeNotFitInConventionException : CacheException
    {
        public TypeNotFitInConventionException(Type type)
            : base(string.Format("type {0} is not configured and does not fit in the convention.", type.FullName))
        {
        }
    }
}
