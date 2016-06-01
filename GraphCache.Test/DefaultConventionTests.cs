using NUnit.Framework;
using GraphCache.Convention;
using GraphCache.Test.DataClasses;
using GraphCache.Exception;

namespace GraphCache.Test
{
    [TestFixture]
    public class DefaultConventionTests
    {
        private DefaultConvention _convetion;

        [SetUp]
        public void SetUp()
        {
            _convetion = new DefaultConvention();
        }

        [Test]
        public void CreateKeyExtractor()
        {
            var keyExtractor = _convetion.CreateKeyExtractor(typeof(Person));

            var person = new Person { Id = 2, Name = "person" };
            var key = keyExtractor(person);

            Assert.AreEqual(person.Id.ToString(), key);
        }

        [Test, ExpectedException(typeof(TypeNotFitInConventionException))]
        public void CreateKeyExtractor_WhenTypeDoNotFitInConvention()
        {
            var keyExtractor = _convetion.CreateKeyExtractor(typeof(City));
        }

        [Test]
        public void FitInConvention()
        {
            var fit = _convetion.FitInConvention(typeof(Person));
            Assert.IsTrue(fit);
        }

        [Test]
        public void FitInConvention_WhenTypeDoNotFitInConvention()
        {
            var fit = _convetion.FitInConvention(typeof(City));
            Assert.IsFalse(fit);
        }
    }
}
