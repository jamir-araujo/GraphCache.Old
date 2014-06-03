using System;

namespace GraphCache
{
    public interface IConvention
    {
        Func<object, string> CreateKeyExtractor(Type type);
        bool FitInConvention(Type type);
    }
}
