using GraphCache.Exceptions;
using System;

namespace GraphCache.Conventions
{
    internal class ConvetionWrapper : Convention
    {
        private Convention _convention;

        public ConvetionWrapper(Convention convention)
        {
            _convention = convention;
        }

        public override Func<object, string> CreateKeyExtractor(Type type)
        {
            try
            {
                return _convention.CreateKeyExtractor(type);
            }
            catch (Exception exception)
            {
                throw new ConventionException(ConventionException.ErroType.CreateKeyExtractorError, exception);
            }
        }

        public override bool FitInConvention(Type type)
        {
            try
            {
                return _convention.FitInConvention(type);
            }
            catch (Exception exception)
            {
                throw new ConventionException(ConventionException.ErroType.FitInConventionError, exception);
            }
        }
    }
}
