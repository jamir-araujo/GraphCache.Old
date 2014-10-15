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
                throw new ArgumentException("duration must be greater than 0 seconds");

            var expiration = this.GetExpirationTime(duration);
            this.AddInternal(value, expiration);
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
                throw new ArgumentException("expirationTime must be greater than current DateTime.now");

            this.AddInternal(value, expirationTime);
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
        /// Returns all objects that satisfies the condition.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="predicate"A function to test each object for a condition.></param>
        /// <returns>An IEnumerable<T> containing all the objects that satisfy the condition.</returns>
        public IEnumerable<T> GetAll<T>(Func<T, bool> predicate)
        {
            var values = _items.OfType<T>().Where(predicate).ToList();
            return this.LoadList(values);
        }

        /// <summary>
        /// Get all the objects of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <returns>An IEnumerable<T> containing all the objects of the specified type.</returns>
        public IEnumerable<T> GetAll<T>()
        {
            var values = _items.OfType<T>().ToList();
            return this.LoadList(values);
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
        /// Removes the first object that satisfies the condition.
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

        /// <summary>
        /// Remove all the objects that satisfies the condition.
        /// </summary>
        /// <typeparam name="T">The type of the objects to remove.</typeparam>
        /// <param name="predicate">A function to test each object for a condition.</param>
        public void RemoveAll<T>(Func<T, bool> predicate)
        {
            var items = _items.OfType<T>().Where(predicate);
            this.RemoveItems(items);
        }

        /// <summary>
        /// Removes all the objects of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the objects to remove.</typeparam>
        public void RemoveAll<T>()
        {
            var items = _items.OfType<T>();
            this.RemoveItems(items);
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
                this.RemoveGraphItem(item, new Dictionary<string, object>());
        }

        /// <summary>
        /// Removes all the object graphs of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the object graphs to remove.</typeparam>
        public void RemoveAllGraphs<T>()
        {
            var items = _items.OfType<T>();
            var removingItems = new Dictionary<string, object>();
            foreach (var item in items)
                this.RemoveGraphItem(item, removingItems);
        }

        /// <summary>
        /// Removes all the object graphs that satisfies de condition.
        /// </summary>
        /// <typeparam name="T">The type of the object graphs to remove.</typeparam>
        /// <param name="predicate">A function to test each object for a condition.</param>
        public void RemoveAllGraphs<T>(Func<T, bool> predicate)
        {
            var items = _items.OfType<T>().Where(predicate);
            var removingItems = new Dictionary<string, object>();
            foreach (var item in items)
                this.RemoveGraphItem(item, removingItems);
        }

        /// <summary>
        /// Remove all objects from the cache.
        /// </summary>
        public void Clear()
        {
            var keys = _cache.Select(p => p.Key);
            foreach (var key in keys)
                _cache.Remove(key);
        }

        private void AddInternal(object value, DateTimeOffset expiration)
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
                this.LoadIEnumerableProperty(property, loadingObjects);
            else
            {
                if (IsValidType(property.Value))
                {
                    var key = this.CreateKey(property.Value);
                    var newValue = this.Get(key, loadingObjects);

                    if (newValue == null)
                        return;

                    var propertyInfo = ownerType.GetProperty(property.Name);
                    propertyInfo.SetValue(ownerValue, newValue, null);
                }
                else
                    LoadObjectProperties(property.Value, loadingObjects);
            }
        }

        private void LoadIEnumerableProperty(Property property, IDictionary<string, object> loadingObjects)
        {
            var list = property.Value as IList;
            if (list == null)
                return;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == null)
                    continue;

                if (IsValidType(list[i]))
                {
                    var key = this.CreateKey(list[i]);
                    var newValue = this.Get(key, loadingObjects);
                    if (newValue != null)
                        list[i] = newValue;
                }
                else
                    LoadObjectProperties(list[i], loadingObjects);
            }
        }

        private List<T> LoadList<T>(List<T> values)
        {
            if (!values.Any())
                return values;

            var loadingObjects = new Dictionary<string, object>();

            for (int i = 0; i < values.Count; i++)
            {
                var key = this.CreateKey(values[i]);
                values[i] = (T)this.Get(key, loadingObjects);
            }

            return values;
        }

        private bool IsIEnumerable(object value)
        {
            return value is IEnumerable;
        }

        private void AddCollection(object value, DateTimeOffset expiration)
        {
            var values = value as IEnumerable;
            foreach (var item in values)
                this.AddInternal(item, expiration);
        }

        private void AddObject(object value, DateTimeOffset expireation)
        {
            if (value == null)
                return;

            if (IsValidType(value))
            {
                var key = this.CreateKey(value);

                if (_cache.Contains(key))
                    return;

                _cache.Add(key, value, expireation);
            }            

            this.AddObjectProperties(value, expireation);
        }

        private bool IsValidType(object value)
        {
            return _configuration.Contains(value.GetType());
        }

        private void AddObjectProperties(object value, DateTimeOffset expireation)
        {
            var cacheableProperties = _objectInspector.GetCacheableProperties(value);
            foreach (var property in cacheableProperties)
                this.AddInternal(property.Value, expireation);
        }

        private void RemoveItems<T>(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                var key = this.CreateKey(item);
                _cache.Remove(key);
            }
        }
        
        private void RemoveGraphItem(object item, IDictionary<string, object> removingItems)
        {
            if (item == null)
                return;

            if (IsValidType(item))
            {
                var key = _keyCreator.CreateKey(item);
                if (removingItems.ContainsKey(key))
                    return;

                _cache.Remove(key);
                removingItems.Add(key, item);
            }

            this.RemoveItemProperties(item, removingItems);
        }

        private void RemoveItemProperties(object item, IDictionary<string, object> removingItems)
        {
            var cacheableProperties = _objectInspector.GetCacheableProperties(item);
            foreach (var property in cacheableProperties)
            {
                if (this.IsIEnumerable(property.Value))
                {
                    var collection = property.Value as IEnumerable;
                    foreach (var collectionItem in collection)
                        this.RemoveGraphItem(collectionItem, removingItems);
                }
                else
                    this.RemoveGraphItem(property.Value, removingItems);
            }
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