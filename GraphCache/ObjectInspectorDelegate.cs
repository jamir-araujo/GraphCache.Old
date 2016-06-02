using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphCache.Helpers;

namespace GraphCache
{
    internal class ObjectInspectorDelegate : IObjectInspector
    {
        public void InspectObject(object value, Action<object> cacheItemFounded)
        {
            Check.NotNull(value, nameof(value));
            Check.NotNull(cacheItemFounded, nameof(cacheItemFounded));

            InspectObject(value, cacheItemFounded, new HashSet<object>());
        }

        public void LoadObject(object value, Func<object, object> cacheItemGetter)
        {
            Check.NotNull(value, nameof(value));
            Check.NotNull(cacheItemGetter, nameof(cacheItemGetter));
            
            LoadObject(value, cacheItemGetter, new HashSet<object>());
        }

        private void InspectObject(object value, Action<object> cacheItemFounded, ICollection<object> workingObjects)
        {
            if (value == null || workingObjects.Contains(value))
                return;

            workingObjects.Add(value);

            if (IsIEnumerable(value))
            {
                InspectIEnumerable((IEnumerable)value, cacheItemFounded, workingObjects);
            }
            else
            {
                cacheItemFounded(value);

                var properties = GetCacheableProperties(value);
                foreach (var property in properties)
                {
                    InspectObject(property.Get(value), cacheItemFounded, workingObjects);
                }
            }
        }

        private IEnumerable<PropertyAssessor> GetCacheableProperties(object value)
        {
            var type = value.GetType();
            return type.GetProperties().Where(IsValidProperty).Select(ConvertToProperty);
        }

        private void InspectIEnumerable(IEnumerable collection, Action<object> cacheItemFounded, ICollection<object> workingObjects)
        {
            foreach (var value in collection)
                InspectObject(value, cacheItemFounded, workingObjects);
        }

        private void LoadObject(object value, Func<object, object> cacheItemGetter, ICollection<object> workingObjects)
        {
            if (value == null || workingObjects.Contains(value))
                return;

            workingObjects.Add(value);

            if (IsIEnumerable(value))
            {
                LoadIEnumerable((IEnumerable)value, cacheItemGetter, workingObjects);
            }
            else
            {
                foreach (var property in GetCacheableProperties(value))
                {
                    var propertyValeu = property.Get(value);
                    if (propertyValeu == null)
                        continue;

                    if (IsIList(propertyValeu))
                    {
                        LoadIList(propertyValeu, cacheItemGetter, workingObjects);
                    }
                    else
                    {
                        var newPropertyValeu = cacheItemGetter(propertyValeu);
                        if (newPropertyValeu != null)
                        {
                            property.Set(value, newPropertyValeu);
                            LoadObject(newPropertyValeu, cacheItemGetter, workingObjects);
                        }
                        else
                        {
                            LoadObject(propertyValeu, cacheItemGetter, workingObjects);
                        }
                    }
                }
            }
        }

        private void LoadIEnumerable(IEnumerable collection, Func<object, object> cacheItemGetter, ICollection<object> workingObjects)
        {
            foreach (var value in collection)
                LoadObject(value, cacheItemGetter, workingObjects);
        }

        private void LoadIList(object value, Func<object, object> cacheItemGetter, ICollection<object> workingObjects)
        {
            var list = value as IList;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == null)
                    continue;

                var newValue = cacheItemGetter(list[i]);
                if (newValue != null)
                {
                    list[i] = newValue;
                }

                LoadObject(list[i], cacheItemGetter, workingObjects);
            }
        }

        private bool IsValidProperty(PropertyInfo propertyInfo)
        {
            if (propertyInfo.PropertyType.IsPrimitive)
                return false;

            if (!propertyInfo.CanRead || !propertyInfo.CanWrite)
                return false;

            if (propertyInfo.PropertyType == typeof(string))
                return false;

            return true;
        }

        private PropertyAssessor ConvertToProperty(PropertyInfo propertyInfo)
        {
            return PropertyAssessorFactory.GetProperty(propertyInfo);
        }

        private bool IsIList(object value)
        {
            return value is IList;
        }

        private bool IsIEnumerable(object value)
        {
            return value is IEnumerable;
        }

        internal class PropertyAssessorFactory
        {
            private static MethodInfo _getPropertyGenericMethod;

            static PropertyAssessorFactory()
            {
                _getPropertyGenericMethod = typeof(PropertyAssessorFactory).GetMethod(nameof(GetGenericProperty), BindingFlags.NonPublic | BindingFlags.Static);
            }

            public static PropertyAssessor GetProperty(PropertyInfo propertyInfo)
            {
                if (PropertyAssessorCache.HasProperty(propertyInfo))
                    return PropertyAssessorCache.Get(propertyInfo);

                var genericMethod = _getPropertyGenericMethod.MakeGenericMethod(propertyInfo.DeclaringType, propertyInfo.PropertyType);
                var assessor = (PropertyAssessor)genericMethod.Invoke(null, new[] { propertyInfo });

                PropertyAssessorCache.Add(assessor);

                return assessor;
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

        public class PropertyAssessor<TOwner, TProperty> : PropertyAssessor
        {
            private Lazy<Getter<TOwner, TProperty>> _getter;
            private Lazy<Setter<TOwner, TProperty>> _setter;

            public PropertyAssessor(
                PropertyInfo propertyInfo,
                Lazy<Getter<TOwner, TProperty>> getter,
                Lazy<Setter<TOwner, TProperty>> setter)
                : base(propertyInfo)
            {
                _getter = getter;
                _setter = setter;
            }

            public override object Get(object owner) => _getter.Value((TOwner)owner);

            public override void Set(object owner, object value) => _setter.Value((TOwner)owner, (TProperty)value);
        }

        public abstract class PropertyAssessor
        {
            public PropertyInfo PropertyInfo { get; private set; }

            public PropertyAssessor(PropertyInfo propertyInfo)
            {
                PropertyInfo = propertyInfo;
            }

            public abstract object Get(object owner);
            public abstract void Set(object owner, object value);
        }
        
        public delegate TProperty Getter<TOwner, TProperty>(TOwner owner);
        public delegate void Setter<TOwner, TProperty>(TOwner owner, TProperty value);

        internal static class PropertyAssessorCache
        {
            private static string KEY_FORMAT = "{0}.{1}";

            private static ConcurrentDictionary<string, PropertyAssessor> _cache = new ConcurrentDictionary<string, PropertyAssessor>();

            public static bool HasProperty(PropertyInfo key)
            {
                var stringKey = CreatePropertyKey(key);
                return _cache.ContainsKey(stringKey);
            }

            public static PropertyAssessor Get(PropertyInfo key)
            {
                var stringKey = CreatePropertyKey(key);
                return _cache[stringKey];
            }

            public static void Add(PropertyAssessor item)
            {
                var stringKey = CreatePropertyKey(item.PropertyInfo);
                _cache.TryAdd(stringKey, item);
            }

            private static string CreatePropertyKey(PropertyInfo propertyInfo)
            {
                return string.Format(KEY_FORMAT, propertyInfo.DeclaringType.FullName, propertyInfo.Name);
            }
        }
    }
}
