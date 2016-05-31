using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace GraphCache.Test
{
    [TestFixture]
    public class ObjectInspectorPerformanceComparasionTests
    {
        private ObjectInspectorDelegate _delegateObjectInspector;
        private ObjectInspector _reflectionObjectInspector;

        [SetUp]
        public void Initialize()
        {
            _reflectionObjectInspector = new ObjectInspector();
            _delegateObjectInspector = new ObjectInspectorDelegate();
        }
    }
}
