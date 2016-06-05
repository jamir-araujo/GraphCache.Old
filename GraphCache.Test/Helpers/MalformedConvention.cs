using System;

namespace GraphCache.Test.Helpers
{
    public class MalformedConvention : Convention
    {
        public override Func<object, string> CreateKeyExtractor(Type type)
        {
            var getter = GetPropertyGetter(type, "NotExistentProperty");
            return value => getter(value).ToString();
        }

        public override bool FitInConvention(Type type)
        {
            return true;
        }
    }
}
