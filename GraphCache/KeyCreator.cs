using GraphCache.Exceptions;
using System;

namespace GraphCache
{
    internal class KeyCreator
    {
        private const string KEY_FORMAT = "{0} = {1}";

        private readonly CacheConfiguration _configuration;

        internal KeyCreator(CacheConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string CreateKey(string partialKey, Type type) => string.Format(KEY_FORMAT, type.FullName, partialKey);

        private string CreatePartialKey(object value)
        {
            var type = value.GetType();
            var keyExtractor = _configuration.GetKeyExtractor(type);
            var key = string.Empty;

            try
            {
                key = keyExtractor(value);
            }
            catch (System.Exception exception)
            {
                throw new KeyExtractorMalformedException(exception);
            }

            return key;
        }

        internal string CreateKey(object value)
        {
            var partialkey = CreatePartialKey(value);
            return CreateKey(partialkey, value.GetType());
        }
    }
}
