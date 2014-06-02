﻿using GraphCache.Exception;
using System;
using System.Reflection;

namespace GraphCache.Convention
{
    internal class DefaultConvention : IConvention
    {
        public Func<object, string> CreateKeyExtractor(Type type)
        {
            var property = type.GetProperty("Id");

            if (property == null)
                throw new TypeNotFitInConventionException(type);

            return this.CreateKeyExtractor(property);
        }

        public bool FitInConvention(Type type)
        {
            var property = type.GetProperty("Id");
            return property != null;
        }

        private Func<object, string> CreateKeyExtractor(PropertyInfo property)
        {
            return obj => property.GetValue(obj).ToString();
        }
    }
}
