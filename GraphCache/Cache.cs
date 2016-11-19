using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using GraphCache.Helpers;

namespace GraphCache
{
    public class Cache
    {
        private readonly CacheConfiguration _configuration;
        private readonly ObjectCache _cache;
        private readonly KeyCreator _keyCreator;
        private readonly IObjectInspector _objectInspector;

        private IEnumerable<object> _items => _cache.Select(p => p.Value);

        /// <summary>
        /// Initializes the cache with the provided configuration.
        /// </summary>
        /// <param name="configuration">The configuration for the cache.</param>
        public Cache(CacheConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            _configuration = configuration;
            _cache = configuration.Cache;
            _keyCreator = new KeyCreator(configuration);
            _objectInspector = new ObjectInspector();
        }

        /// <summary>
        /// Adds an item to the cache.
        /// </summary>
        /// <param name="value">The value to be added. (Can be a derivative of IEnumerable)</param>
        /// <param name="duration">Duration of the item in the cache. (The same duration goes for nested item)</param>
        public void Add(object value, TimeSpan duration)
        {
            Check.NotNull(value, "value");

            if (duration <= default(TimeSpan))
            {
                throw new ArgumentException("duration must be greater than 0 seconds");
            }

            var expiration = GetExpirationTime(duration);
            AddInternal(value, expiration);
        }

        /// <summary>
        /// Adds an item to the cache.
        /// </summary>
        /// <param name="value">The value to be added. (Can be a derivative of IEnumerable)</param>
        /// <param name="expirationTime">The fixed date and time at which the cache entry will expire.</param>
        public void Add(object value, DateTimeOffset expirationTime)
        {
            Check.NotNull(value, "value");

            if (expirationTime <= DateTime.Now)
            {
                throw new ArgumentException("expirationTime must be greater than current DateTime.now");
            }

            AddInternal(value, expirationTime);
        }

        /// <summary>
        /// Returns the first object that satisfies the condition or null if no such object is found.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="predicate">A function to test each object for a condition.</param>
        /// <returns>null if cache is empty or if no object passes the test specified by predicate; otherwise, the first object in source that passes the test specified by predicate.</returns>
        public T Get<T>(Func<T, bool> predicate)
        {
            var value = _items.OfType<T>().FirstOrDefault(predicate);

            if (value == null)
            {
                return value;
            }

            _objectInspector.LoadObject(value, LoadObject);

            return value;
        }

        /// <summary>
        /// Returns all objects that satisfies the condition.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="predicate"A function to test each object for a condition.></param>
        /// <returns>An IEnumerable<T> containing all the objects that satisfy the condition.</returns>
        public IEnumerable<T> GetAll<T>(Func<T, bool> predicate)
        {
            var values = _items.OfType<T>().Where(predicate).ToList();

            _objectInspector.LoadObject(values, LoadObject);

            return values;
        }

        /// <summary>
        /// Get all the objects of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <returns>An IEnumerable<T> containing all the objects of the specified type.</returns>
        public IEnumerable<T> GetAll<T>()
        {
            var values = _items.OfType<T>().ToList();

            _objectInspector.LoadObject(values, LoadObject);

            return values;
        }

        /// <summary>
        /// Determines whether the cache contains any objects that satisfies the condition.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="predicate">A function to test each object for a condition.</param>
        /// <returns>Returns true if any object in the cache passes the test specified by predicate; otherwise returns false.</returns>
        public bool Contains<T>(Func<T, bool> predicate) => _items.OfType<T>().Any(predicate);

        /// <summary>
        /// Removes the first object that satisfies the condition.
        /// </summary>
        /// <typeparam name="T">The type of the object to remove.</typeparam>
        /// <param name="predicate">A function to test each object for a condition.</param>
        public void Remove<T>(Func<T, bool> predicate)
        {
            var item = _items.OfType<T>().FirstOrDefault(predicate);

            if (item == null)
            {
                return;
            }

            var key = CreateKey(item);
            _cache.Remove(key);
        }

        /// <summary>
        /// Remove all the objects that satisfies the condition.
        /// </summary>
        /// <typeparam name="T">The type of the objects to remove.</typeparam>
        /// <param name="predicate">A function to test each object for a condition.</param>
        public void RemoveAll<T>(Func<T, bool> predicate)
        {
            var items = _items.OfType<T>().Where(predicate);
            RemoveItems(items);
        }

        /// <summary>
        /// Removes all the objects of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the objects to remove.</typeparam>
        public void RemoveAll<T>()
        {
            var items = _items.OfType<T>();
            RemoveItems(items);
        }

        /// <summary>
        /// Removes the first object graph that satisfies de condition.
        /// </summary>
        /// <typeparam name="T">The type of the object graph to remove.</typeparam>
        /// <param name="predicate">A function to test each object for a condition.</param>
        public void RemoveGraph<T>(Func<T, bool> predicate)
        {
            var item = _items.OfType<T>().FirstOrDefault(predicate);
            if (item != null)
            {
                _objectInspector.InspectObject(item, RemoveCacheItem);
            }
        }

        /// <summary>
        /// Removes all the object graphs of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the object graphs to remove.</typeparam>
        public void RemoveAllGraphs<T>()
        {
            var items = _items.OfType<T>();
            _objectInspector.InspectObject(items, RemoveCacheItem);
        }

        /// <summary>
        /// Removes all the object graphs that satisfies de condition.
        /// </summary>
        /// <typeparam name="T">The type of the object graphs to remove.</typeparam>
        /// <param name="predicate">A function to test each object for a condition.</param>
        public void RemoveAllGraphs<T>(Func<T, bool> predicate)
        {
            var items = _items.OfType<T>().Where(predicate);
            _objectInspector.InspectObject(items, RemoveCacheItem);
        }

        /// <summary>
        /// Remove all objects from the cache.
        /// </summary>
        public void Clear()
        {
            var keys = _cache.Select(p => p.Key);
            foreach (var key in keys)
            {
                _cache.Remove(key);
            }
        }

        private void AddInternal(object value, DateTimeOffset expiration)
        {
            _objectInspector.InspectObject(value, cacheItem => AddObject(cacheItem, expiration));
        }

        private void AddObject(object value, DateTimeOffset expiration)
        {
            if (IsValidType(value))
            {
                var key = CreateKey(value);
                _cache.Add(key, value, expiration);
            }
        }

        private object LoadObject(object value)
        {
            if (IsValidType(value))
            {
                var key = CreateKey(value);
                value = _cache.Get(key);
            }

            return value;
        }

        private void RemoveCacheItem(object value)
        {
            if (IsValidType(value))
            {
                var key = CreateKey(value);
                _cache.Remove(key);
            }
        }

        private bool IsValidType(object value) => _configuration.Contains(value.GetType());

        private void RemoveItems<T>(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                var key = CreateKey(item);
                _cache.Remove(key);
            }
        }

        private string CreateKey(object value) => _keyCreator.CreateKey(value);

        private DateTimeOffset GetExpirationTime(TimeSpan duration) => DateTimeOffset.Now.Add(duration);
    }
}