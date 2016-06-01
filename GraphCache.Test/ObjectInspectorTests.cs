using GraphCache.Test.DataClasses;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace GraphCache.Test
{
    [TestFixture]
    public class ObjectInspectorTests
    {
        private CacheConfiguration _config;
        private ObjectInspector _objectInspector;

        [SetUp]
        public void Setup()
        {
            _config = new CacheConfiguration(new MemoryCache("ObjectInspectorTests"));
            _objectInspector = new ObjectInspector();
        }

        [Test]
        public void GetCacheableProperties()
        {
            var parentObject = new Parent();
            parentObject.Child = new SimpleClass { Name = "CanReadAndWrite" };

            var cacheables = _objectInspector.GetCacheableProperties(parentObject);

            Assert.AreEqual(1, cacheables.Count());
            var property = cacheables.First();
            Assert.AreEqual("Child", property.Name);
            Assert.AreSame(parentObject.Child, property.Value);
            Assert.IsFalse(cacheables.Any(p => p.Name.Equals("Name")));
            Assert.IsFalse(cacheables.Any(p => p.Name.Equals("CanRead")));
            Assert.IsFalse(cacheables.Any(p => p.Name.Equals("CanWrite")));
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void InspectObject_nullObjectValue()
        {
            _objectInspector.InspectObject(null, (value) => { });
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void InspectObject_nullCacheItemFounded()
        {
            var parentObject = new Parent();
            _objectInspector.InspectObject(parentObject, null);
        }

        [Test]
        public void InspectObject()
        {
            var parentObject = new Parent();
            parentObject.Child = new SimpleClass { Name = "CanReadAndWrite" };
            var foundedObjects = new List<object>();

            _objectInspector.InspectObject(parentObject, (valeu) =>
            {
                foundedObjects.Add(valeu);
            });

            Assert.AreSame(parentObject, foundedObjects[0], "Deveria ter achado o objeto pai");
            Assert.AreSame(parentObject.Child, foundedObjects[1], "Deveria ter achado o objeto da propriedade child");
            Assert.AreEqual(2, foundedObjects.Count, "callBack deveria ser chamado duas vezes");
        }

        [Test]
        public void InspectObject_With_NullProperty()
        {
            var parentObject = new Parent();
            List<Type> foudedTypes = new List<Type>();

            _objectInspector.InspectObject(parentObject, (value) =>
            {
                foudedTypes.Add(value.GetType());
            });

            Assert.IsFalse(foudedTypes.Contains(typeof(SimpleClass)), string.Format("não deveria achado a o callback com a classe {0}", nameof(SimpleClass)));
        }

        [Test]
        public void InspectObject_With_TwoDegreesNavigation()
        {
            var project = CreateProject();
            project.Documents = null;
            var foudedThePersonProperty = false;

            _objectInspector.InspectObject(project, (value) =>
            {
                foudedThePersonProperty = typeof(Person) == value.GetType();
            });

            Assert.IsTrue(foudedThePersonProperty, "Deveria ter achado o tipo Person");
        }

        [Test]
        public void InspectObject_With_IEnumerableProperty()
        {
            var @object = new ClassWithSimpleClassList();
            @object.Id = 1;
            @object.Objects = new List<SimpleClass>
            {
                new SimpleClass { Id = 1, Name = "object 1" },
                new SimpleClass { Id = 2, Name = "object 2" },
                new SimpleClass { Id = 3, Name = "object 3" },
            };
            var childrenFouded = 0;

            _objectInspector.InspectObject(@object, (value) =>
            {
                if (typeof(SimpleClass) == value.GetType())
                    childrenFouded++;
            });

            Assert.AreEqual(3, childrenFouded);
        }

        [Test]
        public void InspectObject_With_IEnumerableProperty_WithNullItem()
        {
            var @object = new ClassWithSimpleClassList();
            @object.Id = 1;
            @object.Objects = new List<SimpleClass>
            {
                new SimpleClass { Id = 1, Name = "object 1" },
                new SimpleClass { Id = 2, Name = "object 2" },
                new SimpleClass { Id = 3, Name = "object 3" },
                null
            };
            var childrenFouded = 0;

            _objectInspector.InspectObject(@object, (value) =>
            {
                if (typeof(SimpleClass) == value.GetType())
                    childrenFouded++;
            });

            Assert.AreEqual(3, childrenFouded);
        }

        [Test]
        public void InspectObject_With_CircularReference()
        {
            var parent = new ClassWithCircularReference_Parent();
            parent.Child = new ClassWithCircularReference_Child();
            parent.Child.Parent = parent;
            var itensFounded = new List<object>();

            _objectInspector.InspectObject(parent, value =>
            {
                itensFounded.Add(value);
            });

            Assert.AreEqual(1, itensFounded.Count(p => p.Equals(parent)), "Deveria ter achado o objeto parent apenas uma vez");
            Assert.AreEqual(1, itensFounded.Count(p => p.Equals(parent.Child)), "Deveria ter achado o objeto parent.Child apenas uma vez");
        }

        [Test]
        public void InspectObject_With_List_InsideItSelf()
        {
            var parentObject = new Parent();
            parentObject.Child = new SimpleClass { Name = "CanReadAndWrite" };
            var list = new List<object>();
            list.Add(parentObject);
            list.Add(list);

            _objectInspector.InspectObject(list, value => { });
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void LoadObject_nullObjectValue()
        {
            _objectInspector.LoadObject(null, value => value);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void LoadObject_nullCacheItemFounded()
        {
            var value = new SimpleClass();
            _objectInspector.LoadObject(value, null);
        }

        [Test]
        public void LoadObject()
        {
            var parent = new Parent();
            var child = new SimpleClass();
            child.Id = 1;
            child.Name = "Name";
            parent.Child = child;

            var newChild = new SimpleClass();
            newChild.Id = 1;
            newChild.Name = "Name Modfied";

            _objectInspector.LoadObject(parent, value =>
            {
                return newChild;
            });

            Assert.AreNotSame(parent.Child, child);
            Assert.AreSame(parent.Child, newChild);
        }

        [Test]
        public void LoadObject_With_NullProperty()
        {
            var parentObject = new Parent();
            var called = false;

            _objectInspector.LoadObject(parentObject, value =>
            {
                called = true;
                return value;
            });

            Assert.IsFalse(called);
        }

        [Test]
        public void LoadObject_With_TwoDegreesNavigation()
        {
            var project = CreateProject();
            project.Documents = null;
            var foudedThePersonProperty = false;

            _objectInspector.LoadObject(project, value =>
            {
                foudedThePersonProperty = typeof(Person) == value.GetType();
                return value;
            });

            Assert.IsTrue(foudedThePersonProperty, "Deveria ter achado o tipo Person");
        }

        [Test]
        public void LoadObject_With_IEnumerableProperty()
        {
            var @object = new ClassWithSimpleClassList();
            @object.Id = 1;
            @object.Objects = new List<SimpleClass>
            {
                new SimpleClass { Id = 1, Name = "object 1" },
                new SimpleClass { Id = 2, Name = "object 2" },
                new SimpleClass { Id = 3, Name = "object 3" },
            };

            var newList = new List<SimpleClass>
            {
                new SimpleClass { Id = 1, Name = "object 1 new" },
                new SimpleClass { Id = 2, Name = "object 2 new" },
                new SimpleClass { Id = 3, Name = "object 3 new" },
            };

            _objectInspector.LoadObject(@object, (value) =>
            {
                var result = value;
                var item = value as SimpleClass;
                if (item != null)
                {
                    result = newList.FirstOrDefault(p => p.Id == item.Id);
                }

                return result;
            });

            Assert.AreSame(newList[0], @object.Objects[0]);
            Assert.AreSame(newList[1], @object.Objects[1]);
            Assert.AreSame(newList[2], @object.Objects[2]);
        }

        [Test]
        public void LoadObject_With_IEnumerableProperty_WithNullItem()
        {
            var @object = new ClassWithSimpleClassList();
            @object.Id = 1;
            @object.Objects = new List<SimpleClass>
            {
                new SimpleClass { Id = 1, Name = "object 1" },
                new SimpleClass { Id = 2, Name = "object 2" },
                new SimpleClass { Id = 3, Name = "object 3" },
                null
            };
            var childrenFouded = 0;

            _objectInspector.LoadObject(@object, (value) =>
            {
                if (typeof(SimpleClass) == value.GetType())
                    childrenFouded++;

                return value;
            });

            Assert.AreEqual(3, childrenFouded);
            Assert.IsNull(@object.Objects[3]);
        }

        [Test]
        public void LoadObject_With_CircularReference()
        {
            var parent = new ClassWithCircularReference_Parent();
            parent.Child = new ClassWithCircularReference_Child();
            parent.Child.Parent = parent;
            var itensFounded = new List<object>();

            _objectInspector.LoadObject(parent, value =>
            {
                itensFounded.Add(value);

                return value;
            });

            Assert.AreEqual(1, itensFounded.Count(p => p.Equals(parent)), "Deveria ter achado o objeto parent apenas uma vez");
            Assert.AreEqual(1, itensFounded.Count(p => p.Equals(parent.Child)), "Deveria ter achado o objeto parent.Child apenas uma vez");
        }

        [Test]
        public void LoadObject_With_List_InsideItSelf()
        {
            var parentObject = new Parent();
            parentObject.Child = new SimpleClass { Name = "CanReadAndWrite" };
            var list = new List<object>();
            list.Add(parentObject);
            list.Add(list);

            _objectInspector.LoadObject(list, value => value);
        }

        [Test]
        public void LoadObject_When_TheDelegateReturnNull()
        {
            var project = CreateProject();
            project.Documents = null;

            var newPerson = new Person();
            newPerson.Id = 1;
            newPerson.Name = "new Person";

            _objectInspector.LoadObject(project, value =>
            {
                if (value is Order)
                    return null;

                if (value is Person)
                    return newPerson;

                return value;
            });

            Assert.AreSame(project.Order.Person, newPerson);
        }

        [Test]
        public void LoadObject_With_IEnumerableProperty_When_TheDelegateReturnNull()
        {
            var @object = new ClassWithComplexClassList();
            @object.Id = 1;
            @object.Objects = new List<ComplexClass>
            {
                new ComplexClass
                {
                    Id = 1,
                    Child = new SimpleClass { Id = 1, Name = "SimpleClass 1" }
                },
                new ComplexClass
                {
                    Id = 2,
                    Child = new SimpleClass { Id =2, Name = "SimpleClass2" }
                }
            };

            var newSimpleClass = new SimpleClass { Id = 1, Name = "SimpleClass2 Modified" };

            _objectInspector.LoadObject(@object, value =>
            {
                if (value is ComplexClass)
                {
                    var complexClass = value as ComplexClass;
                    if (complexClass.Id == 2)
                        return null;
                }

                if (value is SimpleClass)
                {
                    var simpleClass = value as SimpleClass;
                    if (simpleClass.Id == 2)
                        return newSimpleClass;
                }

                return value;
            });

            Assert.AreSame(newSimpleClass, @object.Objects[1].Child);
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

        class ClassWithCircularReference_Parent
        {
            public ClassWithCircularReference_Child Child { get; set; }
        }

        class ClassWithCircularReference_Child
        {
            public ClassWithCircularReference_Parent Parent { get; set; }
        }

        class ClassWithSimpleClassList
        {
            public int Id { get; set; }
            public List<SimpleClass> Objects { get; set; }
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

        class Parent
        {
            private SimpleClass _canRead;
            private SimpleClass _canWrite;

            public SimpleClass Child { get; set; }
            public string Name { get; set; }
            public SimpleClass CanRead { get { return _canRead; } }
            public SimpleClass CanWrite { set { _canWrite = value; } }
        }

        class SimpleClass
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
