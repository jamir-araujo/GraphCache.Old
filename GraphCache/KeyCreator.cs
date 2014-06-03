using GraphCache.Exception;
using System;
using System.Reflection;

namespace GraphCache
{
    internal class KeyCreator
    {
        private readonly CacheConfiguration _configuration;

        internal KeyCreator(CacheConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string CreateKey(string partialKey, Type type)
        {
            return string.Format("{0} = {1}", type.FullName, partialKey);
        }

        private string CreatePartialKey(object value)
        {
            var type = value.GetType().GetTypeInfo();
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
            var partialkey = this.CreatePartialKey(value);
            return this.CreateKey(partialkey, value.GetType());
        }
    }
}
