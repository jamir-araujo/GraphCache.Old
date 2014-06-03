
namespace GraphCache.Exception
{
    internal class KeyExtractorMalformedException : CacheException
    {
        public KeyExtractorMalformedException(System.Exception exception)
            : base("Key Extractor malformed. The Key Extractor created by convection throw an Exception.", exception)
        {
        }
    }
}
