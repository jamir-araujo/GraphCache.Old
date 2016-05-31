using System;

namespace GraphCache
{
    internal interface IObjectInspector
    {
        void InspectObject(object value, Action<object> cacheItemFounded);
        void LoadObject(object value, Func<object, object> cacheItemGetter);
    }
}