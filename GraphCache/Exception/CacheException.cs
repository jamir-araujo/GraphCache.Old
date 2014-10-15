using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace GraphCache.Exception
{
    internal abstract class CacheException : System.Exception
    {
        public CacheException(string message)
            : base(message)
        {
        }

        public CacheException(string message, System.Exception exception) 
            : base(message, exception)
        {
        }
    }
}
