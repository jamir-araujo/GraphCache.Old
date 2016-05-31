using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace GraphCache.Test
{
    [TestFixture]
    public class PropertyCacheTests
    {
        [Test]
        public void HasProperty()
        {
            var propertyInfo = typeof(SimpleClass).GetProperty("Id");
            var property = new ObjectInspectorDelegate.Property<SimpleClass, int>(propertyInfo, null, null);
            ObjectInspectorDelegate.PropertyCache.Add(property);

            Assert.IsTrue(ObjectInspectorDelegate.PropertyCache.HasProperty(propertyInfo));
            
            var newPropertyInfo = typeof(SimpleClass).GetProperty("Id");
            Assert.IsTrue(ObjectInspectorDelegate.PropertyCache.HasProperty(newPropertyInfo));
        }

        [Test]
        public void Get()
        {
            var propertyInfo = typeof(SimpleClass).GetProperty("Id");
            var property = new ObjectInspectorDelegate.Property<SimpleClass, int>(propertyInfo, null, null);
            ObjectInspectorDelegate.PropertyCache.Add(property);

            var cachedProperty = ObjectInspectorDelegate.PropertyCache.Get(propertyInfo);
            Assert.AreSame(property, cachedProperty);
        }

        [Test]
        public void Heritage()
        {
            var childClass_Id_Property = typeof(ChildClass).GetProperty("Id");
            var simpleClass_Id_Property = typeof(SimpleClass).GetProperty("Id");

            Assert.AreSame(simpleClass_Id_Property.DeclaringType, childClass_Id_Property.DeclaringType);
        }

        class SimpleClass
        {
            public int Id { get; set; }
        }

        class ChildClass : SimpleClass
        {

        }
    }
}
