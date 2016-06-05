using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphCache.Exception
{
    internal class TypeNotExpectedException : CacheException
    {
        public TypeNotExpectedException(Type type)
            : base(string.Format("type not expected. The graph cache cannot handle a collection of type {0}", type))
        {
        }
    }
}
