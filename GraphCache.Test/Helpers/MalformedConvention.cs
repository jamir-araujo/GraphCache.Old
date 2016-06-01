using System;

namespace GraphCache.Test.Helpers
{
    public class MalformedConvention : IConvention
    {
        public Func<object, string> CreateKeyExtractor(Type type)
        {
            var property = type.GetProperty("NotExistentProperty");
            return value => property.GetValue(value).ToString();
        }

        public bool FitInConvention(Type type)
        {
            return true;
        }
    }
}
