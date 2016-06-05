using GraphCache.Exceptions;
using GraphCache.Test.DataClasses;
using GraphCache.Test.Helpers;
using Moq;
using NUnit.Framework;
using System;
using System.Runtime.Caching;

namespace GraphCache.Test
{
    [TestFixture]
    public class CacheConfigurationTests
    {
        private CacheConfiguration _config;

        [SetUp]
        public void Setup()
        {
            _config = new CacheConfiguration(new MemoryCache("CacheConfigurationTests"), false);
        }

        [Test]
        public void ConfigureTypeTest()
        {
            _config.ConfigureType<Person>(p => p.Id.ToString());

            Assert.IsTrue(_config.Contains(typeof(Person)));
        }

        [Test]
        public void ContainsTest()
        {
            _config.ConfigureType<Person>(p => p.Id.ToString());

            Assert.IsTrue(_config.Contains(typeof(Person)));
        }

        [Test]
        public void Contains_WhenNotContains()
        {
            Assert.IsFalse(_config.Contains(typeof(Person)));
        }

        [Test]
        public void GetKeyExtractor_WhenTypeIsMapped_ConventionDisabled()
        {
            _config.ConfigureType<Person>(p => p.Id.ToString());
            var keyExtractor = _config.GetKeyExtractor(typeof(Person));
            Assert.IsNotNull(keyExtractor);
            Assert.AreEqual("1", keyExtractor(new Person { Id = 1 }));
        }

        [Test, ExpectedException(typeof(TypeNotMappedException))]
        public void GetKeyExtractor_WhenTypeIsNotMapped_ConventionDisabled()
        {
            _config.GetKeyExtractor(typeof(Person));
        }

        [Test]
        public void GetKeyExtractor_ConventionEnabled()
        {
            _config = new CacheConfiguration(new MemoryCache("CacheConfigurationTests"));
            var keyExtractor = _config.GetKeyExtractor(typeof(Person));
            Assert.IsNotNull(keyExtractor);
            Assert.AreEqual("1", keyExtractor(new Person { Id = 1 }));
        }

        [Test, ExpectedException(typeof(TypeNotFitInConventionException))]
        public void GetKeyExtractor_WhenTypeDoNotFitInConvention()
        {
            _config = new CacheConfiguration(new MemoryCache("CacheConfigurationTests"));
            var keyExtractor = _config.GetKeyExtractor(typeof(City));
        }

        [Test]
        public void GetKeyExtractor_WithCustomConvetion()
        {
            _config = new CacheConfiguration(new MemoryCache("CacheConfigurationTests"), new CityConvention());
            var keyExtractor = _config.GetKeyExtractor(typeof(City));
            var city = new City { Name = "city", PopulationCount = 12345 };
            var key = keyExtractor(city);

            Assert.AreEqual(city.PopulationCount.ToString(), key);
        }

        [Test, ExpectedException(typeof(ConventionException))]
        public void Contains_WhenConvetionThrowsException_OnVerifyFitInConvetion()
        {
            var convention = new Mock<Convention>();
            _config = new CacheConfiguration(new MemoryCache("CacheConfigurationTests"), convention.Object);
            convention.Setup(p => p.FitInConvention(It.IsAny<Type>())).Throws(new System.Exception());

            _config.Contains(typeof(City));
        }

        [Test, ExpectedException(typeof(ConventionException))]
        public void GetKeyExtractor_WhenConventionTrowsException_OnVerifyFitInConvetion()
        {
            var convention = new Mock<Convention>();
            _config = new CacheConfiguration(new MemoryCache("CacheConfigurationTests"), convention.Object);
            convention.Setup(p => p.FitInConvention(It.IsAny<Type>())).Throws(new System.Exception());

            _config.GetKeyExtractor(typeof(City));
        }

        [Test, ExpectedException(typeof(ConventionException))]
        public void GetKeyExtractor_WhenConventionTrowsException_OnCreatingKeyExtractor()
        {
            var convention = new Mock<Convention>();
            _config = new CacheConfiguration(new MemoryCache("CacheConfigurationTests"), convention.Object);
            convention.Setup(p => p.FitInConvention(It.IsAny<Type>())).Returns(true);
            convention.Setup(p => p.CreateKeyExtractor(It.IsAny<Type>())).Throws(new System.Exception());

            _config.GetKeyExtractor(typeof(City));
        }
    }
}
