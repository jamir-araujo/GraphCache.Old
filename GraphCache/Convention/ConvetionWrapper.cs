using GraphCache.Exception;
using System;

namespace GraphCache.Convention
{
    internal class ConvetionWrapper : IConvention
    {
        private IConvention _convention;

        public ConvetionWrapper(IConvention convention)
        {
            _convention = convention;
        }

        public Func<object, string> CreateKeyExtractor(Type type)
        {
            try
            {
                return _convention.CreateKeyExtractor(type);
            }
            catch (System.Exception exception)
            {
                throw new ConventionException(ConventionException.ErroType.CreateKeyExtractorError, exception);
            }
        }

        public bool FitInConvention(Type type)
        {
            try
            {
                return _convention.FitInConvention(type);
            }
            catch (System.Exception exception)
            {
                throw new ConventionException(ConventionException.ErroType.FitInConventionError, exception);
            }
        }
    }
}
