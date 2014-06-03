using GraphCache.Test.DataClasses;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace GraphCache.Test
{
    [TestFixture]
    public class CacheTests
    {
        private CacheConfiguration _config;
        private Cache _cache;

        [SetUp]
        public void Setup()
        {
            _config = new CacheConfiguration(new MemoryCache("CacheTest"));
            _cache = new Cache(_config);
        }

        [Test]
        public void AddAndRetrive()
        {
            var person = new Person();
            person.Id = 1;
            person.Name = "jamir";

            _cache.Add(person, TimeSpan.FromMinutes(10));

            var retrivedPerson = _cache.Get<Person>(p => p.Id == person.Id);

            Assert.AreSame(person, retrivedPerson);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void AddNullObject()
        {
            _cache.Add(null, TimeSpan.FromMinutes(10));
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void Add_WithLessThenZeroDuration()
        {
            _cache.Add(new Person(), TimeSpan.FromMinutes(-1));
        }

        [Test]
        public void AddAndRetrieve_IEnumerableDerived()
        {
            var people = new List<Person> 
            {
                new Person { Id = 1 , Name = "Jamir" },
                new Person { Id = 2 , Name = "Maria" }
            };

            _cache.Add(people, TimeSpan.FromMinutes(10));

            var person1 = _cache.Get<Person>(p => p.Id == people[0].Id);
            var person2 = _cache.Get<Person>(p => p.Id == people[1].Id);

            Assert.AreSame(people[0], person1);
            Assert.AreSame(people[1], person2);
        }

        [Test]
        public void Contains_WhenContains()
        {
            var person = new Person();
            person.Id = 1;
            person.Name = "jamir";

            _cache.Add(person, TimeSpan.FromMinutes(10));

            var contains = _cache.Contains<Person>(p => p.Id == person.Id);

            Assert.IsTrue(contains);
        }

        [Test]
        public void Contains_WhenNotContains()
        {
            var contains = _cache.Contains<Person>(p => p.Id == 1);

            Assert.IsFalse(contains);
        }

        [Test]
        public void RemoveItem()
        {
            var person = new Person();
            person.Id = 1;
            person.Name = "jamir";

            _cache.Add(person, TimeSpan.FromMinutes(10));
            _cache.Remove<Person>(p => p.Id == person.Id);

            Assert.IsFalse(_cache.Contains<Person>(p => p.Id == person.Id));
        }

        [Test]
        public void AddGraph()
        {
            var order = new Order();
            order.Id = 1;
            order.Person = new Person { Id = 2, Name = "jamir" };

            _cache.Add(order, TimeSpan.FromMinutes(10));

            var retrivedOrder = _cache.Get<Order>(p => p.Id == order.Id);
            var retrivedPerson = _cache.Get<Person>(p => p.Id == order.Person.Id);

            Assert.AreEqual(order.Id, retrivedOrder.Id);
            Assert.IsNotNull(retrivedPerson);
            Assert.AreEqual(order.Person.Id, retrivedPerson.Id);
        }

        [Test]
        public void AddGraph_WithNullProperty()
        {
            var order = new Order { Id = 1 };

            _cache.Add(order, TimeSpan.FromMinutes(10));

            var retrivedOrder = _cache.Get<Order>(p => p.Id == order.Id);

            Assert.AreSame(order, retrivedOrder);
            Assert.IsNull(order.Person);
        }

        [Test]
        public void AddGraph_WithList_WithNullItem()
        {
            var author = CreateAuthor();
            author.Books.Insert(0, null);

            _cache.Add(author, TimeSpan.FromMinutes(10));

            var retrivedAuthor = _cache.Get<Author>(p => p.Id == 1);

            Assert.AreSame(author, retrivedAuthor);
            Assert.AreSame(author.Books, retrivedAuthor.Books);
            Assert.IsNull(author.Books[0]);
        }

        [Test]
        public void TryGetValueThatIsNotInTheCache()
        {
            var value = _cache.Get<Person>(p => p.Id == 100);

            Assert.IsNull(value);
        }

        [Test]
        public void GetItem_WithChangedCachedProperties()
        {
            var order = new Order();
            order.Id = 1;
            order.Person = new Person { Id = 2, Name = "jamir" };

            _cache.Add(order, TimeSpan.FromMinutes(10));
            _cache.Remove<Person>(p => p.Id == 2);

            var newPerson = new Person { Id = 2, Name = "Marcos" };
            _cache.Add(newPerson, TimeSpan.FromMinutes(10));

            var cachedOrder = _cache.Get<Order>(p => p.Id == 1);

            Assert.AreSame(newPerson, cachedOrder.Person);
        }

        [Test]
        public void AddGraph_WithIEnumerableProperty()
        {
            var author = CreateAuthor();
            _cache.Add(author, TimeSpan.FromMinutes(10));
        }

        [Test]
        public void GetGraph_WithIEnumerableProperty()
        {
            var author = CreateAuthor();
            _cache.Add(author, TimeSpan.FromMinutes(10));

            var retrivedAuthor = _cache.Get<Author>(p => p.Id == 1);

            Assert.AreSame(author, retrivedAuthor);
            Assert.AreSame(author.Books, retrivedAuthor.Books);
        }

        [Test]
        public void GetGraph_WithChangedIEnumerableProperty()
        {
            var author = CreateAuthor();
            _cache.Add(author, TimeSpan.FromMinutes(10));

            var book = author.Books[3];

            _cache.Remove<Book>(p => p.Id == book.Id);
            var newBook = new Book { Id = book.Id, Title = "livro 4.1", Author = author };

            _cache.Add(newBook, TimeSpan.FromMinutes(10));

            var retrivedAuthor = _cache.Get<Author>(p => p.Id == 1);

            Assert.AreSame(author, retrivedAuthor);
            Assert.IsFalse(retrivedAuthor.Books.Contains(book));
            Assert.Contains(newBook, retrivedAuthor.Books);
            Assert.AreEqual(3, retrivedAuthor.Books.IndexOf(newBook));
        }

        [Test]
        public void AddGraph_WithCircularRefence()
        {
            var user = new User();
            user.Id = 1;
            user.UserName = "Carlos";
            user.Password = "1234";
            user.Profile = new Profile { Id = 2, User = user };

            _cache.Add(user, TimeSpan.FromMinutes(10));
        }

        [Test]
        public void GetGraph_WithCircularRefence()
        {
            AddGraph_WithCircularRefence();

            _cache.Get<User>(p => p.Id == 1);
        }

        private Author CreateAuthor()
        {
            var author = new Author();
            author.Id = 1;
            author.Name = "Carlos";
            author.Books = new List<Book> 
            {
                new Book { Id = 1, Title = "livro 1", Author = author },
                new Book { Id = 2, Title = "livro 2", Author = author },
                new Book { Id = 3, Title = "livro 3", Author = author },
                new Book { Id = 4, Title = "livro 4", Author = author },
                new Book { Id = 5, Title = "livro 5", Author = author }
            };
            return author;
        }
    }
}
