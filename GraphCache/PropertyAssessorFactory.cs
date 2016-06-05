using System;
using System.Reflection;
using GraphCache.Exceptions;

namespace GraphCache
{
    internal static class PropertyAssessorFactory
    {
        private static MethodInfo _getPropertyGenericMethod = typeof(PropertyAssessorFactory)
            .GetMethod(nameof(GetGenericProperty), BindingFlags.NonPublic | BindingFlags.Static);

        public static PropertyAssessor GetProperty(Type type, string propertyName)
        {
            return PropertyAssessorCache.GetOrAdd(type, propertyName, CreatePropertyAssessor);
        }

        public static PropertyAssessor GetProperty(PropertyInfo propertyInfo)
        {
            return PropertyAssessorCache.GetOrAdd(propertyInfo, CreatePropertyAssessor);
        }

        private static PropertyAssessor CreatePropertyAssessor(PropertyKey propertyKey)
        {
            var propertyInfo = propertyKey.PropertyInfo;
            if (propertyInfo == null)
            {
                propertyInfo = propertyKey.DeclaringType.GetProperty(propertyKey.PropertyName);
                if (propertyInfo == null)
                {
                    throw new PropertyNotFoundException(propertyKey.DeclaringType, propertyKey.PropertyName);
                }
            }

            var genericMethod = _getPropertyGenericMethod.MakeGenericMethod(propertyInfo.DeclaringType, propertyInfo.PropertyType);
            return (PropertyAssessor)genericMethod.Invoke(null, new[] { propertyInfo });
        }

        private static PropertyAssessor<TOwner, TProperty> GetGenericProperty<TOwner, TProperty>(PropertyInfo propertyInfo)
        {
            var getter = new Lazy<Getter<TOwner, TProperty>>(() => CreateGetter<TOwner, TProperty>(propertyInfo));
            var setter = new Lazy<Setter<TOwner, TProperty>>(() => CreateSetter<TOwner, TProperty>(propertyInfo));

            return new PropertyAssessor<TOwner, TProperty>(propertyInfo, getter, setter);
        }

        private static Setter<TOwner, TProperty> CreateSetter<TOwner, TProperty>(PropertyInfo propertyInfo)
        {
            var setter = Delegate.CreateDelegate(typeof(Setter<TOwner, TProperty>), propertyInfo.GetSetMethod());
            return (Setter<TOwner, TProperty>)setter;
        }

        private static Getter<TOwner, TProperty> CreateGetter<TOwner, TProperty>(PropertyInfo propertyInfo)
        {
            var getter = Delegate.CreateDelegate(typeof(Getter<TOwner, TProperty>), propertyInfo.GetGetMethod());
            return (Getter<TOwner, TProperty>)getter;
        }
    }
}
