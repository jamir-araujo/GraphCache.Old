using GraphCache.Exception;
using GraphCache.Test.DataClasses;
using GraphCache.Test.Helpers;
using NUnit.Framework;
using System.Runtime.Caching;

namespace GraphCache.Test
{
    [TestFixture]
    public class KeyCreatorTests
    {
        private CacheConfiguration _config;
        private KeyCreator _keyCreator;

        [Test]
        public void CreateKey()
        {
            _config = new CacheConfiguration(new MemoryCache("KeyCreatorTests"));
            _keyCreator = new KeyCreator(_config);

            var person = new Person { Id = 2, Name = "person" };
            var key = this.CreateKey(person, person.Id.ToString());

            var createdKey = _keyCreator.CreateKey(person);

            Assert.AreEqual(key, createdKey);
        }

        [Test, ExpectedException(typeof(KeyExtractorMalformedException))]
        public void CreatePartialKey()
        {
            _config = new CacheConfiguration(new MemoryCache("KeyCreatorTests"), new MalformedConvention());
            _keyCreator = new KeyCreator(_config);

            var person = new Person { Id = 1, Name = "Maria" };

            var createdKey = _keyCreator.CreateKey(person);
        }

        private string CreateKey(object value, string partialKey)
        {
            return string.Format("{0} = {1}", value.GetType().FullName, partialKey);
        }
    }
}
