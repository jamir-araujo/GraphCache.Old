
namespace GraphCache.Exceptions
{
    internal class KeyExtractorMalformedException : CacheException
    {
        public KeyExtractorMalformedException(System.Exception exception)
            : base("Key Extractor malformed. An exception was thrown by keyExtractor created by convention", exception)
        {
        }
    }
}
