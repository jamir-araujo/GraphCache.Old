using System;

namespace GraphCache.Conventions
{
    internal class DefaultConvention : Convention
    {
        public override Func<object, string> CreateKeyExtractor(Type type)
        {
            var getter = GetPropertyGetter(type, "Id");
            return value => getter(value).ToString();
        }

        public override bool FitInConvention(Type type) => HasProperty(type, "Id");
    }
}
