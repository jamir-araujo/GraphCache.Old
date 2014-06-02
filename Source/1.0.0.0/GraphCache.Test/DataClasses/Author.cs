using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphCache.Test.DataClasses
{
    public class Author
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Book> Books { get; set; }
    }
}
