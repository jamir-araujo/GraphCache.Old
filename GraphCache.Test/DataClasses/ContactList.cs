using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphCache.Test.DataClasses
{
    public class ContactList
    {
        public int Id { get; set; }
        public List<Person> People { get; set; }
    }
}
