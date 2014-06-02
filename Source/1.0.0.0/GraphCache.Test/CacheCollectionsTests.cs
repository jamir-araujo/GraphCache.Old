using GraphCache.Exception;
using GraphCache.Test.DataClasses;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Caching;

namespace GraphCache.Test
{
    [TestFixture]
    public class CacheCollectionsTests
    {
        private CacheConfiguration _config;
        private Cache _cache;
        private Person _person4;
        private Person _newPerson3;

        [SetUp]
        public void Setup()
        {
            _config = new CacheConfiguration(new MemoryCache("CacheTest"));
            _cache = new Cache(_config);
            _person4 = null;
            _newPerson3 = null;
        }

        [Test]
        public void List_Test()
        {
            var container = SetUpTestFor<List<Person>>();

            var retrivedContainer = _cache.Get<CollectionContainer<List<Person>>>(p => p.Id == container.Id);

            AssertContainer(retrivedContainer);
        }

        [Test]
        public void Collection_Test()
        {
            var container = SetUpTestFor<Collection<Person>>();
            
            var retrivedContainer = _cache.Get<CollectionContainer<Collection<Person>>>(p => p.Id == container.Id);

            AssertContainer(retrivedContainer);
        }

        [Test]
        public void ObservableCollection_Test()
        {
            var container = SetUpTestFor<ObservableCollection<Person>>();

            var retrivedContainer = _cache.Get<CollectionContainer<ObservableCollection<Person>>>(p => p.Id == container.Id);

            AssertContainer(retrivedContainer);
        }

        [Test]
        public void Array_Test()
        {
            var container = new CollectionContainer<Person[]>();
            container.Id = 1;
            container.Collection = new Person[4];
            container.Collection[0] = new Person { Id = 1, Name = "Person 1" };
            container.Collection[1] = new Person { Id = 2, Name = "Person 2" };
            container.Collection[2] = new Person { Id = 3, Name = "Person 3" };

            _cache.Add(container, TimeSpan.FromMinutes(10));

            var person4 = new Person { Id = 4, Name = "Person 4" };
            container.Collection[3] = person4;

            _cache.Remove<Person>(p => p.Id == 3);
            var newPerson3 = new Person { Id = 3, Name = "Person 3.1" };
            _cache.Add(newPerson3, TimeSpan.FromMinutes(10));

            var retrivedContainer = _cache.Get<CollectionContainer<Person[]>>(p => p.Id == container.Id);

            Assert.IsNotNull(retrivedContainer);
            Assert.Contains(person4, retrivedContainer.Collection);
            Assert.Contains(newPerson3, retrivedContainer.Collection);
        }

        [Test]
        public void ArrayList_Test()
        {
            var container = SetUpTestFor<ArrayList>();

            var retrivedContainer = _cache.Get<CollectionContainer<ArrayList>>(p => p.Id == container.Id);

            AssertContainer(retrivedContainer);
        }

        public CollectionContainer<T> SetUpTestFor<T>() where T : IList, new()
        {
            var container = new CollectionContainer<T>();
            container.Id = 1;
            container.Collection = new T();
            container.Collection.Add(new Person { Id = 1, Name = "Person 1" });
            container.Collection.Add(new Person { Id = 2, Name = "Person 2" });
            container.Collection.Add(new Person { Id = 3, Name = "Person 3" });
            
            _cache.Add(container, TimeSpan.FromMinutes(10));

            _person4 = new Person { Id = 4, Name = "Person 4" };
            container.Collection.Add(_person4);

            _cache.Remove<Person>(p => p.Id == 3);
            _newPerson3 = new Person { Id = 3, Name = "Person 3.1" };
            _cache.Add(_newPerson3, TimeSpan.FromMinutes(10));

            return container;
        }

        private void AssertContainer<T>(CollectionContainer<T> retrivedContainer) where T : IList
        {
            Assert.IsNotNull(retrivedContainer);
            Assert.Contains(_person4, retrivedContainer.Collection);
            Assert.Contains(_newPerson3, retrivedContainer.Collection);
        }
    }
}
