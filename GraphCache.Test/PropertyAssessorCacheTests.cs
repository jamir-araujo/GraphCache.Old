using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace GraphCache.Test
{
    [TestFixture]
    public class PropertyAssessorCacheTests
    {
        private ConcurrentDictionary<PropertyKey, PropertyAssessor> _cache;

        [SetUp]
        public void Initializer()
        {
            _cache = new ConcurrentDictionary<PropertyKey, PropertyAssessor>();
            PropertyAssessorCache._cache = _cache;
        }

        [Test]
        public void HasAssessor_PropertyInfo()
        {
            var propertyInfo = typeof(SimpleClass).GetProperty("Id");
            var propertyKey = new PropertyKey(propertyInfo);
            var assessor = CreatePropertyAssessor(propertyKey);
            _cache.TryAdd(propertyKey, assessor);

            propertyInfo = typeof(SimpleClass).GetProperty("Id");
            var result = PropertyAssessorCache.HasAssessor(propertyInfo);

            Assert.IsTrue(result);
        }

        [Test]
        public void HasAssessor_DeclaringType_And_PropertyName()
        {
            var declaringType = typeof(SimpleClass);
            var propertyName = "Id";
            var propertyKey = new PropertyKey(declaringType, propertyName);
            var assessor = CreatePropertyAssessor(propertyKey);
            _cache.TryAdd(propertyKey, assessor);

            var result = PropertyAssessorCache.HasAssessor(typeof(SimpleClass), "Id");

            Assert.IsTrue(result);
        }

        [Test]
        public void GetOrAdd_PropertyInfo()
        {
            var propertyInfo = typeof(SimpleClass).GetProperty("Id");
            var created = false;

            var assessor = PropertyAssessorCache.GetOrAdd(propertyInfo, propertyKey =>
            {
                created = true;
                return CreatePropertyAssessor(propertyKey);
            });

            propertyInfo = typeof(SimpleClass).GetProperty("Id");
            Assert.IsTrue(_cache.ContainsKey(new PropertyKey(propertyInfo)));
            Assert.IsTrue(created);
        }

        [Test]
        public void GetOrAdd_PropertyInfo_WhenIsAlreadyInCache()
        {
            var propertyInfo = typeof(SimpleClass).GetProperty("Id");
            var created = false;
            var propertyKey = new PropertyKey(propertyInfo);

            _cache.TryAdd(propertyKey, CreatePropertyAssessor(propertyKey));

            propertyInfo = typeof(SimpleClass).GetProperty("Id");
            var assessor = PropertyAssessorCache.GetOrAdd(propertyInfo, propertyKeyForCreation =>
            {
                created = true;
                return CreatePropertyAssessor(propertyKeyForCreation);
            });

            propertyInfo = typeof(SimpleClass).GetProperty("Id");
            Assert.IsTrue(_cache.ContainsKey(new PropertyKey(propertyInfo)));
            Assert.IsFalse(created);
        }

        [Test]
        public void GetOrAdd_DeclaringType_And_PropertyName()
        {
            var declaringType = typeof(SimpleClass);
            var propertyName = "Id";
            var created = false;

            var assessor = PropertyAssessorCache.GetOrAdd(declaringType, propertyName, propertyKey =>
            {
                created = true;
                return CreatePropertyAssessor(propertyKey);
            });

            Assert.IsTrue(_cache.ContainsKey(new PropertyKey(typeof(SimpleClass), "Id")));
            Assert.IsTrue(created);
        }

        [Test]
        public void GetOrAdd_DeclaringTypeAndPropertyName_WhenIsAlreadyInCache()
        {
            var propertyKey = new PropertyKey(typeof(SimpleClass), "Id");
            _cache.TryAdd(propertyKey, CreatePropertyAssessor(propertyKey));
            var created = false;

            var assessor = PropertyAssessorCache.GetOrAdd(typeof(SimpleClass), "Id", propertyKeyForCreation =>
            {
                created = true;
                return CreatePropertyAssessor(propertyKeyForCreation);
            });

            Assert.IsTrue(_cache.ContainsKey(new PropertyKey(typeof(SimpleClass), "Id")));
            Assert.IsFalse(created);
        }

        private PropertyAssessor CreatePropertyAssessor(PropertyKey propertyKey)
        {
            var propertyInfo = propertyKey.PropertyInfo;
            if (propertyInfo == null)
            {
                propertyInfo = propertyKey.DeclaringType.GetProperty(propertyKey.PropertyName);
            }

            return new PropertyAssessor<SimpleClass, int>(propertyInfo,
                new Lazy<Getter<SimpleClass, int>>(() => null),
                new Lazy<Setter<SimpleClass, int>>(() => null));
        }

        private string GetKey(PropertyInfo key)
        {
            return GetKey(key.DeclaringType, key.Name);
        }

        private string GetKey(Type declaringType, string propertyName)
        {
            return string.Format("{0}.{1}", declaringType.FullName, propertyName);
        }

        class SimpleClass
        {
            public int Id { get; set; }
        }

        class ChildClass : SimpleClass
        {

        }
    }
}
