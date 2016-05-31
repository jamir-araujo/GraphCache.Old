using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Reflection;

namespace GraphCache.Test
{
    [TestFixture]
    public class ReflectionTests
    {
        public delegate object GetMethod(object value);

        private MethodInfo _delegateCreator = typeof(ReflectionTests).GetTypeInfo().GetDeclaredMethod(nameof(CreateDelegate));

        private Func<Person, string> GetMethodDelegate;

        public void Get()
        {
            var type = typeof(Person);
            var property = type.GetProperty("Name");

            //SetGetMethod(type, property);
            SetGetMethod(property);

            var normalResult = NormalReflection();
            var delegateResult = DelegateReflection();
            var normalAccess = NormalAccess();

            var normalReflectionResult = string.Format("Normal Reflection: {0}", normalResult);
            var delegateReflectionResult = string.Format("Delegate Reflection: {0}", delegateResult);
            var normalAccessResult = string.Format("Normal Access: {0}", normalAccess);
            var diferenca = string.Format("Delegate é {0} mais rápido", (normalResult.Ticks / delegateResult.Ticks));

            var a = new string[] { normalReflectionResult, delegateReflectionResult, normalAccessResult, diferenca };
        }

        //private void SetGetMethod(Type type, PropertyInfo property)
        //{
        //    var genericMethod = _delegateCreator.MakeGenericMethod(type, property.PropertyType);
        //    GetMethodDelegate = (Func<object, object>)genericMethod.Invoke(this, new[] { property.GetGetMethod() });
        //}

        private void SetGetMethod(PropertyInfo property)
        {
            GetMethodDelegate = CreateDelegate<Person, string>(property.GetGetMethod());
        }

        private TimeSpan DelegateReflection()
        {
            var person = new Person();
            person.Name = "Jamir";

            var stopWhatch = new Stopwatch();
            stopWhatch.Start();

            for (int i = 0; i < 10000; i++)
            {
                GetMethodDelegate(person);
            }

            stopWhatch.Stop();

            return stopWhatch.Elapsed;
        }

        private TimeSpan NormalReflection()
        {
            var stopWhatch = new Stopwatch();

            var person = new Person();
            person.Name = "Jamir";

            stopWhatch.Start();

            var type = person.GetType();
            var property = type.GetProperty("Name");

            for (int i = 0; i < 10000; i++)
            {
                property.GetValue(person);
            }

            stopWhatch.Stop();

            return stopWhatch.Elapsed;
        }

        private TimeSpan NormalAccess()
        {
            var stopWhatch = new Stopwatch();

            var person = new Person();
            person.Name = "Jamir";

            stopWhatch.Start();

            for (int i = 0; i < 10000; i++)
            {
                var name = person.Name;
            }

            stopWhatch.Stop();

            return stopWhatch.Elapsed;
        }

        private Func<TType, TProperty> CreateDelegate<TType, TProperty>(MethodInfo methodInfo)
        {
            return (Func<TType, TProperty>)Delegate.CreateDelegate(typeof(Func<TType, TProperty>), methodInfo);
            //var func = (Func<TType, TProperty>)methodInfo.CreateDelegate(typeof(Func<TType, TProperty>));
            //return (owner) => func((TType)owner);
        }

        class Person
        {
            public string Name { get; set; }
        }
    }
}
