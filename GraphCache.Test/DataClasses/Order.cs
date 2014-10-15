using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphCache.Test.DataClasses
{
    public class Order
    {
        public int Id { get; set; }
        public Person Person { get; set; }
        public string Key { get; set; }
    }
}
