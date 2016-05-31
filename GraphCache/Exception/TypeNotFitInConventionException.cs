﻿using System;

namespace GraphCache.Exception
{
    internal class TypeNotFitInConventionException : CacheException
    {
        public TypeNotFitInConventionException(Type type)
            : base(string.Format("type {0} is not configured and does not fit in the convention.", type.FullName))
        {
        }
    }
}
