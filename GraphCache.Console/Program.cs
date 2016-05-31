using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace GraphCache.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var cache = new Cache(new CacheConfiguration(new MemoryCache("Testes")));

            var person = new Person();
            person.Id = 1;
            person.Address = new Address
            {
                Id = 1,
                Description = "adlhfalkjdshf"
            };
            person.Phones = new List<Phone>
            {
                new Phone { Id =1, Number = "999999999999", Type = PhoneType.Home },
                new Phone { Id =2, Number = "999999999999", Type = PhoneType.Home },
                new Phone { Id =3, Number = "999999999999", Type = PhoneType.Home },
                new Phone { Id =4, Number = "999999999999", Type = PhoneType.Home },
                new Phone { Id =5, Number = "999999999999", Type = PhoneType.Home },
                new Phone { Id =6, Number = "999999999999", Type = PhoneType.Home },
                new Phone { Id =7, Number = "999999999999", Type = PhoneType.Home },
                new Phone { Id =8, Number = "999999999999", Type = PhoneType.Home },
                new Phone { Id =9, Number = "999999999999", Type = PhoneType.Home },
                new Phone { Id =10, Number = "999999999999", Type = PhoneType.Home },
                new Phone { Id =11, Number = "999999999999", Type = PhoneType.Home },
                new Phone { Id =12, Number = "999999999999", Type = PhoneType.Home },
                new Phone { Id =13, Number = "999999999999", Type = PhoneType.Home },
            };

            cache.Add(cache, TimeSpan.FromHours(1));

            var cachePerson = cache.Get<Person>(p => p.Id == 1);

            cache.Remove<Person>(p => p.Id == 1);
            cache.Add(cache, TimeSpan.FromHours(1));

            System.Console.WriteLine("Done!!");
            System.Console.ReadKey();
        }
    }

    public class Person
    {
        public long Id { get; set; }
        public Address Address { get; set; }
        public List<Phone> Phones { get; set; }
    }

    public class Address
    {
        public long Id { get; set; }
        public string Description { get; set; }
    }

    public class Phone
    {
        public long Id { get; set; }
        public string Number { get; set; }
        public PhoneType Type { get; set; }
    }

    public enum PhoneType
    {
        Home,
        Job
    }
}
