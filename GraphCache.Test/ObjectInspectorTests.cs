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
            _objectInspector = new ObjectInspector();
        }

        [Test]
        public void GetCacheableProperties()
        {
            var parentObject = new Parent();
            parentObject.Children = new Children { Name = "CanReadAndWrite" };

            var cacheables = _objectInspector.GetCacheableProperties(parentObject);

            Assert.AreEqual(1, cacheables.Count());
            var property = cacheables.First();
            Assert.AreEqual("Children", property.Name);
            Assert.AreSame(parentObject.Children, property.Value);
            Assert.IsFalse(cacheables.Any(p => p.Name.Equals("Name")));
            Assert.IsFalse(cacheables.Any(p => p.Name.Equals("CanRead")));
            Assert.IsFalse(cacheables.Any(p => p.Name.Equals("CanWrite")));
        }

        class Parent
        {
            private Children _canRead;
            private Children _canWrite;

            public Children Children { get; set; }
            public string Name { get; set; }
            public Children CanRead { get { return _canRead; } }
            public Children CanWrite { set { _canWrite = value; } }
        }

        class Children
        {
            public string Name { get; set; }
        }
    }
}
