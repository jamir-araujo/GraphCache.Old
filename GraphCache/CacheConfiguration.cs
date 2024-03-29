﻿using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using GraphCache.Conventions;
using GraphCache.Exceptions;
using GraphCache.Helpers;

namespace GraphCache
{
    public class CacheConfiguration
    {
        private readonly bool _conventionEnabled;
        private readonly ObjectCache _cache;
        private readonly Convention _convention;
        private readonly Dictionary<Type, Func<object, string>> _configuredTypes;

        internal ObjectCache Cache => _cache;

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
            _convention = new ConvetionWrapper(new DefaultConvention());
            _configuredTypes = new Dictionary<Type, Func<object, string>>();
        }

        /// <summary>
        /// Initializes the configuration with the provided ObjectCache and the custom convention.
        /// </summary>
        /// <param name="objectCache">The internal ObjectCache that will store the values</param>
        /// <param name="convention">A custom convention that will create the keyExtractors</param>
        public CacheConfiguration(ObjectCache objectCache, Convention convention)
        {
            Check.NotNull(objectCache, "objectCachem");
            Check.NotNull(convention, "convention");

            _cache = objectCache;
            _convention = new ConvetionWrapper(convention);
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
            var wrappedExtractor = WrapExtractor(keyExtractor);
            _configuredTypes.Add(typeof(T), wrappedExtractor);
        }

        internal bool Contains(Type type)
        {
            return _configuredTypes.ContainsKey(type) || FitInConvention(type);
        }

        internal Func<object, string> GetKeyExtractor(Type type)
        {
            if (_configuredTypes.ContainsKey(type))
            {
                return _configuredTypes[type];
            }

            if (!_conventionEnabled)
            {
                throw new TypeNotMappedException(type);
            }

            var keyExtractor = CreateKeyExtractor(type);
            _configuredTypes.Add(type, keyExtractor);

            return keyExtractor;
        }

        private Func<object, string> CreateKeyExtractor(Type type)
        {
            if (!FitInConvention(type))
            {
                throw new TypeNotFitInConventionException(type);
            }

            return _convention.CreateKeyExtractor(type);
        }

        private bool FitInConvention(Type type)
        {
            if (_conventionEnabled)
            {
                return _convention.FitInConvention(type);
            }

            return false;
        }

        private Func<object, string> WrapExtractor<T>(Func<T, string> keyExtractor) => valeu => keyExtractor((T)valeu);
    }
}
