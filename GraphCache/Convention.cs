using System;

namespace GraphCache
{
    public abstract class Convention
    {
        public abstract Func<object, string> CreateKeyExtractor(Type type);

        public abstract bool FitInConvention(Type type);

        protected Func<object, object> GetPropertyGetter(Type type, string propertyName)
        {
            return PropertyAssessorFactory.GetProperty(type, propertyName).GetValue;
        }

        protected bool HasProperty(Type type, string propertyName)
        {
            var hasProperty = PropertyAssessorCache.HasAssessor(type, propertyName);
            if (hasProperty)
            {
                return hasProperty;
            }

            return type.GetProperty(propertyName) != null;
        }
    }
}
