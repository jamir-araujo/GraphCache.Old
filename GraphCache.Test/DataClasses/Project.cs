using System.Collections.Generic;

namespace GraphCache.Test.DataClasses
{
    public class Project
    {
        public int Id { get; set; }
        public List<Document> Documents { get; set; }
        public Order Order { get; set; }
    }
}
