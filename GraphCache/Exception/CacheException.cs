namespace GraphCache.Exception
{
    internal abstract class CacheException : System.Exception
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
