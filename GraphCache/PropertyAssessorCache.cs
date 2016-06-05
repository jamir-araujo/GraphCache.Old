using System;
using System.Collections.Concurrent;
using System.Reflection;
using GraphCache.Exceptions;

namespace GraphCache
{
    internal static class PropertyAssessorCache
    {
        internal static ConcurrentDictionary<PropertyKey, PropertyAssessor> _cache = new ConcurrentDictionary<PropertyKey, PropertyAssessor>();

        public static bool HasAssessor(PropertyInfo propertyInfo)
        {
            var propertyKey = new PropertyKey(propertyInfo);
            return _cache.ContainsKey(propertyKey);
        }

        public static bool HasAssessor(Type declaringType, string propertyName)
        {
            var propertyKey = new PropertyKey(declaringType, propertyName);
            return _cache.ContainsKey(propertyKey);
        }

        public static PropertyAssessor GetOrAdd(PropertyInfo propertyInfo, Func<PropertyKey, PropertyAssessor> createPropertyAssessor)
        {
            var propertyKey = new PropertyKey(propertyInfo);
            return _cache.GetOrAdd(propertyKey, createPropertyAssessor);
        }

        public static PropertyAssessor GetOrAdd(Type declaringType, string propertyName, Func<PropertyKey, PropertyAssessor> createPropertyAssessor)
        {
            var propertyKey = new PropertyKey(declaringType, propertyName);
            return _cache.GetOrAdd(propertyKey, createPropertyAssessor);
        }
    }

    internal struct PropertyKey : IEquatable<PropertyKey>
    {
        private const string KEY_FORMAT = "{0}.{1}";

        private readonly int _hashcode;

        public PropertyInfo PropertyInfo { get; }
        public Type DeclaringType { get; }
        public string PropertyName { get; }

        public PropertyKey(PropertyInfo propertyInfo)
        {
            DeclaringType = propertyInfo.DeclaringType;
            PropertyName = propertyInfo.Name;
            PropertyInfo = propertyInfo;

            _hashcode = CalculateHashCode(DeclaringType, PropertyName);
        }

        public PropertyKey(Type declaringType, string propertyName)
        {
            DeclaringType = declaringType;
            PropertyName = propertyName;
            PropertyInfo = null;

            _hashcode = CalculateHashCode(DeclaringType, PropertyName);
        }

        public static bool operator ==(PropertyKey left, PropertyKey right) => Equals(left, right);

        public static bool operator !=(PropertyKey left, PropertyKey right) => !Equals(left, right);

        public bool Equals(PropertyKey other)
        {
            return DeclaringType == other.DeclaringType && PropertyName == other.PropertyName;
        }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) &&
                   (ReferenceEquals(this, obj) || (obj is PropertyKey) && Equals((PropertyKey)obj));
        }

        public override int GetHashCode() => _hashcode;

        public override string ToString()
        {
            return ToString(DeclaringType, PropertyName);
        }

        private static string ToString(Type declaringType, string propertyName)
        {
            return string.Format(KEY_FORMAT, declaringType.FullName, propertyName);
        }

        private static int CalculateHashCode(Type declaringType, string propertyName)
        {
            return unchecked(declaringType.GetHashCode() * 397) ^ propertyName.GetHashCode();
        }
    }
}
