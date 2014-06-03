using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphCache.Exception
{
    internal class TypeNotMappedException : CacheException
    {
        public TypeNotMappedException(Type type)
            : base(string.Format("type {0} is not configured", type.FullName))
        {
        }
    }
}
