using GraphCache.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace GraphCache
{
    public class Cache
    {
        private readonly CacheConfiguration _configuration;
        private readonly ObjectCache _cache;
        private readonly KeyCreator _keyCreator;
        private readonly ObjectInspector _objectInspector;

        private IEnumerable<object> _items
        {
            get { return _cache.Select(p => p.Value); }
        }

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
            _objectInspector = new ObjectInspector(configuration);
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
                throw new ArgumentException("duration must be greater than 0 seconds");

            var expiration = this.GetExpirationTime(duration);
            this.Add(value, expiration);
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
                return value;

            var key = this.CreateKey(value);
            value = (T)this.Get(key, new Dictionary<string, object>());

            return value;
        }

        /// <summary>
        /// Determines whether the cache contains any objects that satisfies the condition.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="predicate">A function to test each object for a condition.</param>
        /// <returns>Returns true if any object in the cache passes the test specified by predicate; otherwise returns false.</returns>
        public bool Contains<T>(Func<T, bool> predicate)
        {
            return _items.OfType<T>().Any(predicate);
        }

        /// <summary>
        /// Removes the first object that satisfies the condition
        /// </summary>
        /// <typeparam name="T">The type of the object to remove.</typeparam>
        /// <param name="predicate">A function to test each object for a condition.</param>
        public void Remove<T>(Func<T, bool> predicate)
        {
            var item = _items.OfType<T>().FirstOrDefault(predicate);

            if (item == null)
                return;

            var key = this.CreateKey(item);
            _cache.Remove(key);
        }

        private void Add(object value, DateTimeOffset expiration)
        {
            if (IsIEnumerable(value))
                this.AddCollection(value, expiration);
            else
                this.AddObject(value, expiration);
        }

        private object Get(string key, IDictionary<string, object> loadingObjects)
        {
            if (loadingObjects.ContainsKey(key))
                return loadingObjects[key];

            var value = _cache.Get(key);
            if (value == null)
                return value;

            loadingObjects.Add(key, value);

            this.LoadObjectProperties(value, loadingObjects);

            return value;
        }

        private void LoadObjectProperties(object value, IDictionary<string, object> loadingObjects)
        {
            var cacheableProperties = _objectInspector.GetCacheableProperties(value);
            var type = value.GetType();
            foreach (var cacheableProperty in cacheableProperties)
                this.LoadProperty(cacheableProperty, value, type, loadingObjects);
        }

        private void LoadProperty(Property property, object ownerValue, Type ownerType, IDictionary<string, object> loadingObjects)
        {
            if (property.Value == null)
                return;

            if (IsIEnumerable(property.Value))
                this.LoadPropertyIEnumerable(property, loadingObjects);
            else
            {
                var key = this.CreateKey(property.Value);
                var newValue = this.Get(key, loadingObjects);

                if (newValue == null)
                    return;

                var propertyInfo = ownerType.GetProperty(property.Name);
                propertyInfo.SetValue(ownerValue, newValue);
            }
        }

        private void LoadPropertyIEnumerable(Property property, IDictionary<string, object> loadingObjects)
        {
            var list = property.Value as IList;
            if (list == null)
                return;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == null)
                    continue;

                var key = this.CreateKey(list[i]);
                var newValue = this.Get(key, loadingObjects);
                if (newValue != null)
                    list[i] = newValue;
            }
        }

        private bool IsIEnumerable(object value)
        {
            return value is IEnumerable;
        }

        private void AddCollection(object value, DateTimeOffset expiration)
        {
            var values = value as IEnumerable;
            foreach (var item in values)
                this.Add(item, expiration);
        }

        private void AddObject(object value, DateTimeOffset expireation)
        {
            if (value == null)
                return;

            var key = this.CreateKey(value);

            if (_cache.Contains(key))
                return;

            _cache.Add(key, value, expireation);

            this.AddObjectProperties(value, expireation);
        }

        private void AddObjectProperties(object value, DateTimeOffset expireation)
        {
            var cacheableProperties = _objectInspector.GetCacheableProperties(value);
            foreach (var property in cacheableProperties)
                this.Add(property.Value, expireation);
        }

        private string CreateKey(object value)
        {
            return _keyCreator.CreateKey(value);
        }

        private DateTimeOffset GetExpirationTime(TimeSpan duration)
        {
            return DateTime.Now + duration;
        }
    }
}
