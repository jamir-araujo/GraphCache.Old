namespace GraphCache.Exceptions
{
    internal class ConventionException : CacheException
    {
        private readonly ErroType _type;
        private const string DEFAULT_MESSAGE = "An Exception was thrown on call the {0} method of the convention, see the InnerException for more details";
        private readonly string CREATE_KEY_EXTRACTOR_ERROR_MESSAGE = string.Format(DEFAULT_MESSAGE, "CreateKeyExtractor");
        private readonly string FIT_IN_CONVENTION_ERROR_MESSAGE = string.Format(DEFAULT_MESSAGE, "FitInConvention");

        public override string Message
        {
            get
            {
                if (_type == ErroType.CreateKeyExtractorError)
                    return CREATE_KEY_EXTRACTOR_ERROR_MESSAGE;
                else
                    return FIT_IN_CONVENTION_ERROR_MESSAGE;
            }
        }

        internal ConventionException(ErroType type, System.Exception exception)
            : base(string.Empty, exception)
        {
            _type = type;
        }

        internal enum ErroType
        {
            CreateKeyExtractorError,
            FitInConventionError
        }
    }
}
