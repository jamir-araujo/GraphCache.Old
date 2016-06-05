using System;

namespace GraphCache.Test.Helpers
{
    public class CityConvention : Convention
    {
        public override Func<object, string> CreateKeyExtractor(Type type)
        {
            var getter = GetPropertyGetter(type, "PopulationCount");
            return value => getter(value).ToString();
        }

        public override bool FitInConvention(Type type)
        {
            return HasProperty(type, "PopulationCount");
        }
    }
}
