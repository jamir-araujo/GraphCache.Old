using GraphCache.Test.DataClasses;
using NUnit.Framework;
using System.Linq;
using System.Runtime.Caching;

namespace GraphCache.Test
{
    [TestFixture]
    public class ObjectInspectorTests
    {
        private CacheConfiguration _config;
        private ObjectInspector _objectInspector;

        [SetUp]
        public void Setup()
        {
            _config = new CacheConfiguration(new MemoryCache("ObjectInspectorTests"));
            _objectInspector = new ObjectInspector(_config);
        }

        [Test]
        public void GetCacheableProperties()
        {
            var order = new Order();
            order.Id = 1;
            order.Person = new Person { Id = 1, Name = "Roberto" };

            var cacheables = _objectInspector.GetCacheableProperties(order);

            Assert.AreEqual(1, cacheables.Count());
            Assert.AreSame(order.Person, cacheables.First().Value);
            Assert.AreEqual("Person", cacheables.First().Name);
        }
    }
}
