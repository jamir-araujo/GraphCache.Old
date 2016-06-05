using GraphCache.Test.DataClasses;
using NUnit.Framework;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace GraphCache.Test
{
    [TestFixture]
    public class CacheTests
    {
        private CacheConfiguration _config;
        private Cache _cache;
        private MemoryCache _internalCache;

        [SetUp]
        public void Setup()
        {
            _internalCache = new MemoryCache("CacheTest");
            _config = new CacheConfiguration(_internalCache);
            _cache = new Cache(_config);
        }

        [Test]
        public void AddItem_WithDoration()
        {
            var person = new Person { Id = 1, Name = "person" };

            _cache.Add(person, TimeSpan.FromMinutes(10));

            var retrivedPerson = _internalCache.First().Value;

            Assert.AreSame(person, retrivedPerson);
        }

        [Test]
        public void AddItem_WithExpiration()
        {
            var person = new Person { Id = 1, Name = "person" };

            _cache.Add(person, DateTime.Now + TimeSpan.FromMinutes(10));

            var retrivedPerson = _internalCache.First().Value;

            Assert.AreSame(person, retrivedPerson);
        }

        [Test]
        public void GetItem()
        {
            var person = new Person { Id = 1, Name = "person" };
            var key = new KeyCreator(_config).CreateKey(person);

            _internalCache.Add(key, person, DateTime.Now + TimeSpan.FromMinutes(1));

            var retrivedPerson = _cache.Get<Person>(p => p.Id == person.Id);

            Assert.AreSame(person, retrivedPerson);
        }

        [Test]
        public void GetItems()
        {
            var keyCreator = new KeyCreator(_config);
            var people = new List<Person> 
            {
                new Person { Id = 1, Name = "person 1" },
                new Person { Id = 2, Name = "person 2" },
                new Person { Id = 3, Name = "person 3" }
            };

            foreach (var person in people)
            {
                var key = keyCreator.CreateKey(person);
                _internalCache.Add(key, person, DateTime.Now + TimeSpan.FromMinutes(10));
            }

            var retrivedPeople = _cache.GetAll<Person>(p => p.Name.StartsWith("person")).ToList();

            Assert.Contains(people[0], retrivedPeople);
            Assert.Contains(people[1], retrivedPeople);
            Assert.Contains(people[2], retrivedPeople);
        }

        [Test]
        public void GetAll()
        {
            var keyCreator = new KeyCreator(_config);
            var people = new List<Person> 
            {
                new Person { Id = 1, Name = "person 1" },
                new Person { Id = 2, Name = "person 2" },
                new Person { Id = 3, Name = "person 3" }
            };

            foreach (var person in people)
            {
                var key = keyCreator.CreateKey(person);
                _internalCache.Add(key, person, DateTime.Now + TimeSpan.FromMinutes(10));
            }

            var retrivedPeople = _cache.GetAll<Person>().ToList();

            Assert.Contains(people[0], retrivedPeople);
            Assert.Contains(people[1], retrivedPeople);
            Assert.Contains(people[2], retrivedPeople);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void AddNullObject()
        {
            _cache.Add(null, TimeSpan.FromMinutes(10));
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void AddItem_WithDuration_WhenDurationIsLessThenZero()
        {
            _cache.Add(new Person(), TimeSpan.FromMinutes(-1));
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void AddItem_WithExpiration_WhenExpirationTimeIsLessThenNow()
        {
            _cache.Add(new Person(), DateTime.Now);
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

            var items = _internalCache.Select(p => p.Value).ToList();

            Assert.Contains(people[0], items);
            Assert.Contains(people[1], items);
        }

        [Test]
        public void Contains_WhenContains()
        {
            var person = new Person();
            person.Id = 1;
            person.Name = "jamir";

            _internalCache.Add("Peron", person, DateTime.Now + TimeSpan.FromMinutes(10));

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
            var person = new Person { Id = 1, Name = "jamir" };

            var keyCreator = new KeyCreator(_config);
            var key = keyCreator.CreateKey(person);

            _internalCache.Add(key, person, DateTime.Now + TimeSpan.FromMinutes(10));

            _cache.Remove<Person>(p => p.Id == person.Id);

            Assert.IsFalse(_internalCache.Contains(key));
        }

        [Test]
        public void RemoveAll_WithPredicate()
        {
            var keyCreator = new KeyCreator(_config);
            var people = new List<Person> 
            { 
                new Person { Id = 1, Name = "person 1" },
                new Person { Id = 2, Name = "person 2" },
                new Person { Id = 3, Name = "person 3" },
                new Person { Id = 4, Name = "person 4" }
            };

            foreach (var person in people)
            {
                var key = keyCreator.CreateKey(person);
                _internalCache.Add(key, person, DateTime.Now + TimeSpan.FromMinutes(10));
            }

            _cache.RemoveAll<Person>(p => p.Name.StartsWith("person"));

            foreach (var person in people)
                Assert.IsFalse(_internalCache.Contains(keyCreator.CreateKey(person)));
        }

        [Test]
        public void RemoveAll()
        {
            var keyCreator = new KeyCreator(_config);
            var people = new List<Person> 
            { 
                new Person { Id = 1, Name = "person 1" },
                new Person { Id = 2, Name = "person 2" },
                new Person { Id = 3, Name = "person 3" },
                new Person { Id = 4, Name = "person 4" }
            };

            foreach (var person in people)
            {
                var key = keyCreator.CreateKey(person);
                _internalCache.Add(key, person, DateTime.Now + TimeSpan.FromMinutes(10));
            }

            _cache.RemoveAll<Person>();

            foreach (var person in people)
                Assert.IsFalse(_internalCache.Contains(keyCreator.CreateKey(person)));
        }

        [Test]
        public void RemoveGraph()
        {
            var order = new Order();
            order.Id = 1;
            order.Person = new Person { Id = 2, Name = "jamir" };
            _cache.Add(order, TimeSpan.FromMinutes(10));

            _cache.RemoveGraph<Order>(p => p.Id == order.Id);

            Assert.IsFalse(_cache.Contains<Order>(p => p.Id == order.Id));
            Assert.IsFalse(_cache.Contains<Person>(p => p.Id == order.Person.Id));
        }

        [Test]
        public void RemoveGraph_WithNullProperty()
        {
            var order = new Order { Id = 1 };
            _cache.Add(order, TimeSpan.FromMinutes(10));

            _cache.RemoveGraph<Order>(p => p.Id == order.Id);

            Assert.IsFalse(_cache.Contains<Order>(p => p.Id == order.Id));
        }

        [Test]
        public void RemoveGraph_WithIEnumerableProperty()
        {
            var author = CreateAuthor();
            author.Books.ForEach(p => p.Author = null);
            _cache.Add(author, TimeSpan.FromMinutes(10));

            _cache.RemoveGraph<Author>(p => p.Id == author.Id);

            Assert.IsFalse(_cache.Contains<Author>(p => p.Id == author.Id));
            foreach (var book in author.Books)
                Assert.IsFalse(_cache.Contains<Book>(p => p.Id == book.Id));
        }

        [Test]
        public void RemoveGraph_NullIEnumerableProperty()
        {
            var author = CreateAuthor();
            author.Books = null;
            _cache.Add(author, TimeSpan.FromMinutes(10));

            _cache.RemoveGraph<Author>(p => p.Id == author.Id);

            Assert.IsFalse(_cache.Contains<Author>(p => p.Id == author.Id));
        }

        [Test]
        public void RemoveGraph_WithCircularReference()
        {
            var author = CreateAuthor();
            _cache.Add(author, TimeSpan.FromMinutes(10));

            _cache.RemoveGraph<Author>(p => p.Id == author.Id);

            Assert.IsFalse(_cache.Contains<Author>(p => p.Id == author.Id));
            foreach (var book in author.Books)
                Assert.IsFalse(_cache.Contains<Book>(p => p.Id == book.Id));
        }

        [Test]
        public void RemoveGraph_WithNonConventionalObjectIterruption()
        {
            var task = CreateTask();
            _cache.Add(task, TimeSpan.FromMinutes(10));

            _cache.RemoveGraph<Task>(p => p.Id == task.Id);

            Assert.IsFalse(_cache.Contains<Task>(p => p.Id == 1));
            Assert.IsFalse(_cache.Contains<Order>(p => p.Id == 1));
            Assert.IsFalse(_cache.Contains<Person>(p => p.Id == 2));
            Assert.IsFalse(_cache.Contains<Person>(p => p.Id == 1));
        }

        [Test]
        public void RemoveGraph_WithListOfNonConventionalObjectIterruption()
        {
            var projetc = CreateProject();
            _cache.Add(projetc, TimeSpan.FromMinutes(10));

            _cache.RemoveGraph<Project>(p => p.Id == projetc.Id);

            Assert.IsFalse(_cache.Contains<Project>(p => p.Id == 1));
            Assert.IsFalse(_cache.Contains<Order>(p => p.Id == 1));
            Assert.IsFalse(_cache.Contains<Person>(p => p.Id == 1));
            Assert.IsFalse(_cache.Contains<Person>(p => p.Id == 2));
            Assert.IsFalse(_cache.Contains<Person>(p => p.Id == 3));
            Assert.IsFalse(_cache.Contains<Person>(p => p.Id == 4));
        }

        [Test]
        public void RevemoGraph_WithNonConvetionnalObjectInterrruptionContainCircularReferece()
        {
            var root = GetUnconvetionlObject();
            _cache.Add(root, TimeSpan.FromMinutes(10));

            _cache.RemoveGraph<Root>(p => p.Id == root.Id);

            Assert.IsFalse(_cache.Contains<Root>(p => p.Id == 1));
            Assert.IsFalse(_cache.Contains<Convetional>(p => p.Id == 1));
            Assert.IsFalse(_cache.Contains<Person>(p => p.Id == 1));
        }

        [Test]
        public void RemoveAllGraphs_WithPredicate()
        {
            var orders = new List<Order> 
            {
                new Order
                {
                    Id = 1,
                    Person = new Person { Id = 1, Name = "person 1" },
                    Key = "key"
                },
                new Order
                {
                    Id = 2,
                    Person = new Person { Id = 2, Name = "person 2" },
                    Key = "key"
                },
                new Order
                {
                    Id = 3,
                    Person = new Person { Id = 3, Name = "person 3" },
                    Key = "key"
                }
            };
            _cache.Add(orders, TimeSpan.FromMinutes(10));

            _cache.RemoveAllGraphs<Order>(p => p.Key == "key");

            Assert.IsFalse(_cache.Contains<Order>(p => p.Id == 1));
            Assert.IsFalse(_cache.Contains<Person>(p => p.Id == 1));
            Assert.IsFalse(_cache.Contains<Order>(p => p.Id == 2));
            Assert.IsFalse(_cache.Contains<Person>(p => p.Id == 2));
            Assert.IsFalse(_cache.Contains<Order>(p => p.Id == 3));
            Assert.IsFalse(_cache.Contains<Person>(p => p.Id == 3));
        }

        [Test]
        public void RemoveAllGraphs()
        {
            var orders = new List<Order> 
            {
                new Order
                {
                    Id = 1,
                    Person = new Person { Id = 1, Name = "person 1" }
                },
                new Order
                {
                    Id = 2,
                    Person = new Person { Id = 2, Name = "person 2" }
                },
                new Order
                {
                    Id = 3,
                    Person = new Person { Id = 3, Name = "person 3" }
                }
            };

            _cache.Add(orders, TimeSpan.FromMinutes(10));

            _cache.RemoveAllGraphs<Order>();

            Assert.IsFalse(_cache.Contains<Order>(p => p.Id == 1));
            Assert.IsFalse(_cache.Contains<Person>(p => p.Id == 1));
            Assert.IsFalse(_cache.Contains<Order>(p => p.Id == 2));
            Assert.IsFalse(_cache.Contains<Person>(p => p.Id == 2));
            Assert.IsFalse(_cache.Contains<Order>(p => p.Id == 3));
            Assert.IsFalse(_cache.Contains<Person>(p => p.Id == 3));
        }

        [Test]
        public void Clear()
        {
            var keyCreator = new KeyCreator(_config);
            var items = new List<object> 
            { 
                new Person { Id = 1, Name = "person 1" },
                new Person { Id = 2, Name = "person 2" },
                new Person { Id = 3, Name = "person 3" },
                new Book { Id = 1, Title = "book 1" },
                new Book { Id = 2, Title = "book 2" },
                new Book { Id = 3, Title = "book 3" }
            };

            foreach (var item in items)
            {
                var key = keyCreator.CreateKey(item);
                _internalCache.Add(key, item, DateTime.Now + TimeSpan.FromMinutes(10));
            }

            _cache.Clear();

            foreach (var item in items)
                Assert.IsFalse(_internalCache.Contains(keyCreator.CreateKey(item)));
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
        public void AddGraph_WithIEnumerableProperty()
        {
            var author = CreateAuthor();
            _cache.Add(author, TimeSpan.FromMinutes(10));
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

        [Test]
        public void AddGraph_WithNonConventionalObjectIterruption()
        {
            var task = CreateTask();

            _cache.Add(task, TimeSpan.FromMinutes(10));

            Assert.IsTrue(_cache.Contains<Task>(p => p.Id == 1));
            Assert.IsTrue(_cache.Contains<Order>(p => p.Id == 1));
            Assert.IsTrue(_cache.Contains<Person>(p => p.Id == 2));
            Assert.IsTrue(_cache.Contains<Person>(p => p.Id == 1));
        }

        [Test]
        public void GetGraph_WithNonConventionalObjectIterruption()
        {
            var task = CreateTask();

            _cache.Add(task, TimeSpan.FromMinutes(10));
            _cache.Remove<Person>(p => p.Id == task.Document.Uploader.Id);

            var uploader2 = new Person { Id = 1, Name = "uploader 2" };
            _cache.Add(uploader2, TimeSpan.FromMinutes(10));

            var retrivedTask = _cache.Get<Task>(p => p.Id == task.Id);

            Assert.AreEqual(uploader2, task.Document.Uploader);
        }

        [Test]
        public void AddGraph_WithListOfNonConventionalObjectIterruption()
        {
            var project = CreateProject();

            _cache.Add(project, TimeSpan.FromMinutes(10));

            Assert.IsTrue(_cache.Contains<Project>(p => p.Id == 1));
            Assert.IsTrue(_cache.Contains<Order>(p => p.Id == 1));
            Assert.IsTrue(_cache.Contains<Person>(p => p.Id == 1));
            Assert.IsTrue(_cache.Contains<Person>(p => p.Id == 2));
            Assert.IsTrue(_cache.Contains<Person>(p => p.Id == 3));
            Assert.IsTrue(_cache.Contains<Person>(p => p.Id == 4));
        }

        [Test]
        public void GetGraph_WithListOfNonConventionalObjectIterruption()
        {
            var projetc = CreateProject();

            _cache.Add(projetc, TimeSpan.FromMinutes(10));
            _cache.Remove<Person>(p => p.Id == projetc.Documents[1].Uploader.Id);

            var uploader2 = new Person { Id = 2, Name = "uploader 2.1" };
            _cache.Add(uploader2, TimeSpan.FromMinutes(10));

            var retrivedProject = _cache.Get<Project>(p => p.Id == projetc.Id);

            Assert.AreEqual(uploader2, projetc.Documents[1].Uploader);
        }

        private Task CreateTask()
        {
            var task = new Task
            {
                Id = 1,
                Document = new Document
                {
                    Name = "file.txt",
                    Uploader = new Person { Id = 1, Name = "uploader" }
                },
                Order = new Order
                {
                    Id = 1,
                    Person = new Person { Id = 2, Name = "order owner" }
                }
            };
            return task;
        }

        private Project CreateProject()
        {
            return new Project
            {
                Id = 1,
                Documents = new List<Document> 
                {
                    new Document
                    {
                        Name = "document 1",
                        Uploader = new Person { Id = 1, Name = "person 1" }
                    },
                    new Document
                    {
                        Name = "document 2",
                        Uploader = new Person { Id = 2, Name = "person 2" }
                    },
                    new Document
                    {
                        Name = "document 3",
                        Uploader = new Person { Id = 3, Name = "person 2" }
                    }
                },
                Order = new Order
                {
                    Id = 1,
                    Person = new Person { Id = 4, Name = "person 4" }
                }
            };
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

        private Root GetUnconvetionlObject()
        {
            var root = new Root();
            root.Id = 1;
            root.Unconventionl = new Unconvetional();
            root.Unconventionl.Convetional = new Convetional();
            root.Unconventionl.Convetional.Id = 1;
            root.Unconventionl.Convetional.Person = new Person { Id = 1, Name = "Person" };
            root.Unconventionl.Convetional.Parent = root.Unconventionl;

            return root;
        }
    }

    public class Root
    {
        public int Id { get; set; }
        public Unconvetional Unconventionl { get; set; }
    }

    public class Unconvetional
    {
        public Convetional Convetional { get; set; }
    }

    public class Convetional
    {
        public int Id { get; set; }
        public Unconvetional Parent { get; set; }
        public Person Person { get; set; }
    }
}