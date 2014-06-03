using System.Collections;

namespace GraphCache.Test.DataClasses
{
    public class CollectionContainer<T>
    {
        public int Id { get; set; }
        public T Collection { get; set; }
    }
}
