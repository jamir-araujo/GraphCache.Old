using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphCache.Test.Helpers
{
    public class CityConvention : IConvention
    {
        public Func<object, string> CreateKeyExtractor(Type type)
        {
            var property = type.GetProperty("PopulationCount");
            return @object => property.GetValue(@object).ToString();
        }

        public bool FitInConvention(Type type)
        {
            return type.GetProperty("PopulationCount") != null;
        }
    }
}
