using System;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace GraphCache.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var iterations = 100000;
            var arg1 = string.Empty;
            if (args.Length != 0)
            {
                arg1 = args[0];
                switch (arg1)
                {
                    case "CPU":
                        iterations = 100000;
                        break;
                    case "MEMORY":
                        iterations = 5000;
                        break;
                    default:
                        break;
                }
            }

            var time = DateTime.Now.Add(TimeSpan.FromMinutes(10));
            var cache = new MemoryCache("GraphCache.Console");
            var @object = GetObject();
            System.Console.WriteLine("runing {0} iterations for {1} test", iterations, arg1);
            for (int i = 0; i < iterations; i++)
            {
                RunTest(time, cache, @object);

                if ((i % 1000) == 0)
                {
                    System.Console.WriteLine("{0} iterations", i);
                }
            }
        }

        static void RunTest(DateTime time, MemoryCache internalCache, ClassWithComplexClassList @object)
        {
            var cache = new Cache(new CacheConfiguration(internalCache));

            cache.Add(@object, time);

            var cacheItem = cache.Get<ClassWithComplexClassList>(item => item.Id == 1);
            cache.RemoveAllGraphs<ClassWithComplexClassList>();
        }

        static ClassWithComplexClassList GetObject()
        {
            var @object = new ClassWithComplexClassList();
            @object.Id = 1;
            @object.Objects = new List<ComplexClass>
            {
                new ComplexClass
                {
                    Id = 1,
                    Child = new SimpleClass { Id = 1, Name = "SimpleClass1" }
                },
                new ComplexClass
                {
                    Id = 2,
                    Child = new SimpleClass { Id =2, Name = "SimpleClass2" }
                },
                new ComplexClass
                {
                    Id = 2,
                    Child = new SimpleClass { Id =3, Name = "SimpleClass3" }
                },
                new ComplexClass
                {
                    Id = 2,
                    Child = new SimpleClass { Id =4, Name = "SimpleClass4" }
                },
                new ComplexClass
                {
                    Id = 2,
                    Child = new SimpleClass { Id =5, Name = "SimpleClass5" }
                },
                new ComplexClass
                {
                    Id = 2,
                    Child = new SimpleClass { Id =6, Name = "SimpleClass6" }
                }
            };

            return @object;
        }
    }

    class ClassWithComplexClassList
    {
        public int Id { get; set; }
        public List<ComplexClass> Objects { get; set; }
    }

    class ComplexClass
    {
        public int Id { get; set; }
        public SimpleClass Child { get; set; }
    }

    class SimpleClass
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
