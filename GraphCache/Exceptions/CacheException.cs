using System;

namespace GraphCache.Exceptions
{
    internal abstract class CacheException : Exception
    {
        public CacheException(string message)
            : base(message)
        {
        }

        public CacheException(string message, System.Exception exception) 
            : base(message, exception)
        {
        }
    }
}
