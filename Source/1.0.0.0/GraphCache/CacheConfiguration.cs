using GraphCache.Convention;
using GraphCache.Exception;
using GraphCache.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;

namespace GraphCache
{
    public class CacheConfiguration
    {
        private readonly bool _conventionEnabled;
        private readonly ObjectCache _cache;
        private readonly IConvention _convention;
        private readonly Dictionary<Type, Func<object, string>> _configuredTypes;

        internal ObjectCache Cache
        {
            get { return _cache; }
        }

        /// <summary>
        /// Initializes the configuration with the provided ObjectCache and the default convention enabled.
        /// </summary>
        /// <param name="objectCache">The internal ObjectCache that will store the values</param>
        public CacheConfiguration(ObjectCache objectCache)
            : this(objectCache, true)
        {
        }

        /// <summary>
        /// Initializes the configuration with the provided ObjectCache and indicates if convention is enabled.
        /// </summary>
        /// <param name="objectCache">The internal ObjectCache that will store the values</param>
        /// <param name="convetionEnabled">Indicates whether the Convention will be enabled or not. (the default value is true)</param>
        public CacheConfiguration(ObjectCache objectCache, bool convetionEnabled)
        {
            Check.NotNull(objectCache, "objectCachem");

            _cache = objectCache;
            _conventionEnabled = convetionEnabled;
            _convention = new DefaultConvention();
            _configuredTypes = new Dictionary<Type, Func<object, string>>();
        }

        /// <summary>
        /// Initializes the configuration with the provided ObjectCache and the custom convention.
        /// </summary>
        /// <param name="objectCache">The internal ObjectCache that will store the values</param>
        /// <param name="convention">A custom convention that will create the keyExtractors</param>
        public CacheConfiguration(ObjectCache objectCache, IConvention convention)
        {
            Check.NotNull(objectCache, "objectCachem");
            Check.NotNull(convention, "convention");

            _cache = objectCache;
            _convention = convention;
            _conventionEnabled = true;
            _configuredTypes = new Dictionary<Type, Func<object, string>>();
        }

        /// <summary>
        /// Configures a keyExtractor that creates a unique key for each object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the object that you want to configure.</typeparam>
        /// <param name="keyExtractor">A function that returns a string that is unique for each object of the specified type.</param>
        public void ConfigureType<T>(Func<T, string> keyExtractor)
        {
            var wrappedExtractor = this.WrapExtractor(keyExtractor);
            _configuredTypes.Add(typeof(T), wrappedExtractor);
        }

        internal bool Contains(Type type)
        {
            if (IsIEnumerable(type))
            {
                if (IsValidCollection(type))
                    type = this.ExtractElementType(type);
                else
                    return false;
            }

            return _configuredTypes.ContainsKey(type) || FitInConvention(type);
        }

        internal Func<object, string> GetKeyExtractor(Type type)
        {
            if (_configuredTypes.ContainsKey(type))
                return _configuredTypes[type];

            if (!_conventionEnabled)
                throw new TypeNotMappedException(type);

            var keyExtractor = this.CreateKeyExtractor(type);
            _configuredTypes.Add(type, keyExtractor);

            return keyExtractor;
        }

        private bool IsIEnumerable(Type type)
        {
            return type.GetInterfaces().Any(i => i == typeof(IEnumerable));
        }

        private Type ExtractElementType(Type type)
        {
            Type elementType = null;

            if (type.IsGenericType)
                elementType = type.GetGenericArguments()[0];
            else if (type.IsArray)
                elementType = type.GetElementType();
            else
                throw new TypeNotExpectedException(type);

            return elementType;
        }

        private bool IsValidCollection(Type type)
        {
            return type.GetInterfaces().Any(i => i == typeof(IList));
        }

        private Func<object, string> CreateKeyExtractor(Type type)
        {
            if (!FitInConvention(type))
                throw new TypeNotFitInConventionException(type);

            return _convention.CreateKeyExtractor(type);
        }

        private bool FitInConvention(Type type)
        {
            if (_conventionEnabled)
                return _convention.FitInConvention(type);
            return false;
        }

        private Func<object, string> WrapExtractor<T>(Func<T, string> keyExtractor)
        {
            return valeu => keyExtractor((T)valeu);
        }
    }
}
