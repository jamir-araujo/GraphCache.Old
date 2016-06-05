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
                    InspectObject(property.GetValue(value), cacheItemFounded, workingObjects);
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
                    var propertyValeu = property.GetValue(value);
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
                            property.SetValue(value, newPropertyValeu);
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
    }
}
