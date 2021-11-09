using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Newtonsoft.Json;
using Moq;
using Redacted;
using Redacted.Configuration;
using System.Collections.Generic;

namespace Redacted.Tests
{
    [TestClass]
    public class RedactorTests
    {
        [ClassInitialize]
        public static void ClassInitializer(TestContext context)
        {
        }

        #region Constructor
        [TestMethod]
        public void Constuctor_WhenConfigIsNotNull_ShouldInstantiateNewRedactor()
        {
            var nameConfig = RedactedResource.GetMockedRedactorConfiguration(RedactBy.Name);
            var patternConfig = RedactedResource.GetMockedRedactorConfiguration(RedactBy.Pattern);
            var nameAndPatternConfig = RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern);

            var nameRedactor = new Redactor(nameConfig);
            var patternRedactor = new Redactor(patternConfig);
            var nameAndPatternRedactor = new Redactor(nameAndPatternConfig);

            Assert.AreNotEqual(null, nameRedactor);
            Assert.AreNotEqual(null, patternRedactor);
            Assert.AreNotEqual(null, nameAndPatternRedactor);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WhenConfigIsNull_ShouldThrowArgumentNullException()
        {
            _ = new Redactor(null);
        }
        #endregion


        #region Redact
        /// <summary>
        /// Testing <see cref="Redactor.Redact(string)"/> high level functionality, nothing except RedactorConfiguration is mocked or faked.
        /// </summary>
        [TestMethod]
        public void Redact_WhenValueToRedactHasXmlEdgesAndIsValidXml_ShouldReturnRedactedXmlValue()
        {
            var valueToRedact = RedactedResource.Get("Redactor_InnerJSON", "xml");
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.Name));

            var returnValue = redactor.Redact(valueToRedact);

            Assert.AreEqual(valueToRedact, returnValue);
        }

        /// <summary>
        /// Testing <see cref="Redactor.Redact(string)"/> high level functionality, nothing except RedactorConfiguration is mocked or faked.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Redact_WhenValueToRedactHasXmlEdgesAndIsNotValidXml_ShouldThrowArgumentException()
        {
            var valueToRedact = "<Not XML>>";
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.Name));

            _ = redactor.Redact(valueToRedact);
        }

        /// <summary>
        /// Testing <see cref="Redactor.Redact(string)"/> high level functionality, nothing except RedactorConfiguration is mocked or faked.
        /// </summary>
        [TestMethod]
        public void Redact_WhenValueToRedactHasJsonEdgesAndIsValidJson_ShouldReturnRedactedJsonValue()
        {

        }

        /// <summary>
        /// Testing <see cref="Redactor.Redact(string)"/> high level functionality, nothing except RedactorConfiguration is mocked or faked.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Redact_WhenValueToRedactHasJsonEdgesAndIsNotValidJson_ShouldThrowArgumentException()
        {
            var valueToRedact = "{Not JSON}}";
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.Name));

            _ = redactor.Redact(valueToRedact);
        }

        /// <summary>
        /// Testing <see cref="Redactor.Redact(string)"/> high level functionality, nothing except RedactorConfiguration is mocked or faked.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Redact_WhenValueToRedactDoesNotHaveXmlOrJsonEdges_ShouldThrowArgumentException()
        {
            var valueToRedact = "Not XML or JSON";
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.Name));

            _ = redactor.Redact(valueToRedact);
        }
        #endregion
    }
}
