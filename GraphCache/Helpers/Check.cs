﻿using System;

namespace GraphCache.Helpers
{
    internal class Check
    {
        internal static T NotNull<T>(T value, string parameterName) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            return value;
        }
    }
}
