using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphCache.Helpers;

namespace GraphCache
{
    internal class ObjectInspector : IObjectInspector
    {
        public void InspectObject(object value, Action<object> cacheItemFounded)
        {
            Check.NotNull(value, nameof(value));
            Check.NotNull(cacheItemFounded, nameof(cacheItemFounded));

            InspectObject(value, cacheItemFounded, new List<object>());
        }

        public void LoadObject(object value, Func<object, object> cacheItemGetter)
        {
            Check.NotNull(value, nameof(value));
            Check.NotNull(cacheItemGetter, nameof(cacheItemGetter));

            LoadObject(value, cacheItemGetter, new List<object>());
        }

        internal IEnumerable<Property> GetCacheableProperties(object value)
        {
            var cacheableProperties = new List<Property>();

            var type = value.GetType();
            var properties = type.GetProperties().Where(IsValidProperty);

            foreach (var property in properties)
            {
                cacheableProperties.Add(new Property
                {
                    Name = property.Name,
                    Value = property.GetValue(value, null)
                });
            }

            return cacheableProperties;
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
                var type = value.GetType();
                var properties = GetCacheableProperties(value);
                foreach (var property in properties)
                {
                    if (property.Value == null)
                        continue;

                    if (IsIList(property.Value))
                    {
                        LoadIList(property.Value, cacheItemGetter, workingObjects);
                    }
                    else
                    {
                        LoadProperty(property, value, type, cacheItemGetter, workingObjects);
                    }
                }
            }
        }

        private void LoadIEnumerable(IEnumerable collection, Func<object, object> cacheItemGetter, ICollection<object> workingObjects)
        {
            foreach (var value in collection)
                LoadObject(value, cacheItemGetter, workingObjects);
        }

        private void LoadProperty(Property property, object ownerValue, Type ownerType, Func<object, object> cacheItemGetter, ICollection<object> workingObjects)
        {
            var propertyInfo = ownerType.GetProperty(property.Name);
            var newPropertyValue = cacheItemGetter(property.Value);
            if (newPropertyValue != null)
            {
                propertyInfo.SetValue(ownerValue, newPropertyValue, null);
                LoadObject(newPropertyValue, cacheItemGetter, workingObjects);
            }
            else
            {
                LoadObject(property.Value, cacheItemGetter, workingObjects);
            }
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
                    InspectObject(property.Value, cacheItemFounded, workingObjects);
                }
            }
        }

        private void InspectIEnumerable(IEnumerable collection, Action<object> cacheItemFounded, ICollection<object> workingObjects)
        {
            foreach (var value in collection)
                InspectObject(value, cacheItemFounded, workingObjects);
        }

        private bool IsIEnumerable(object value)
        {
            return value is IEnumerable;
        }

        private bool IsIList(object value)
        {
            return value is IList;
        }
    }
}
