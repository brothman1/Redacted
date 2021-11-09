using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Newtonsoft.Json.Linq;
using Redacted.Fakes;

namespace Redacted.Tests
{
    [TestClass]
    public class JsonRedactorTests
    {
        #region Constructor
        [TestMethod]
        public void Constuctor_WhenConfigIsNotNull_ShouldInstantiateNewRedactor()
        {
            var nameConfig = RedactedResource.GetMockedRedactorConfiguration(RedactBy.Name);
            var patternConfig = RedactedResource.GetMockedRedactorConfiguration(RedactBy.Pattern);
            var nameAndPatternConfig = RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern);

            var nameJsonRedactor = new JsonRedactor(nameConfig);
            var patternJsonRedactor = new JsonRedactor(patternConfig);
            var nameAndPatternJsonRedactor = new JsonRedactor(nameAndPatternConfig);

            Assert.AreNotEqual(null, nameJsonRedactor);
            Assert.AreNotEqual(null, patternJsonRedactor);
            Assert.AreNotEqual(null, nameAndPatternJsonRedactor);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WhenConfigIsNull_ShouldThrowArgumentNullException()
        {
            _ = new JsonRedactor(null);
        }
        #endregion

        #region Methods
        #region Redact
        [TestMethod]
        public void Redact_WhenJsonToRedactIsValidJson_ShouldReturnRedactedJsonValue()
        {
            var jsonToRedact = RedactedResource.Get("Redactor");
            var expectedValue = RedactedResource.Get("Redactor_Redacted");
            var JsonRedactor = new JsonRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));

            var returnValue = JsonRedactor.Redact(jsonToRedact);

            Assert.AreEqual(expectedValue.RemoveWhitespace(), returnValue.RemoveWhitespace());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Redact_WhenJsonToRedactIsNotValidJson_ShouldThrowArgumentException()
        {
            var jsonToRedact = "{Not Json}}";
            var JsonRedactor = new JsonRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));

            _ = JsonRedactor.Redact(jsonToRedact);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Redact_WhenJsonToRedactIsNull_ShouldThrowArgumentException()
        {
            var jsonToRedact = null as string;
            var JsonRedactor = new JsonRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));

            _ = JsonRedactor.Redact(jsonToRedact);
        }
        #endregion

        #region GetJson
        [TestMethod]
        public void GetJson_WhenJsonToRedactIsValidJson_ShouldReturnDeserializedJson()
        {
            var jsonToRedact = RedactedResource.Get("Redactor");
            var jsonRedactor = new JsonRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
            var privatejsonRedactor = new PrivateObject(jsonRedactor);

            var returnValue = privatejsonRedactor.Invoke("GetJson", jsonToRedact) as JToken;

            Assert.AreNotEqual(null, returnValue);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetJson_WhenJsonToRedactIsNotValidJson_ShouldThrowArgumentException()
        {
            var jsonToRedact = "{Not Json}}";
            var jsonRedactor = new JsonRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
            var privatejsonRedactor = new PrivateObject(jsonRedactor);

            _ = privatejsonRedactor.Invoke("GetJson", jsonToRedact);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetJson_WhenJsonToRedactIsNull_ShouldThrowArgumentException()
        {
            var jsonToRedact = null as string;
            var jsonRedactor = new JsonRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
            var privatejsonRedactor = new PrivateObject(jsonRedactor);

            _ = privatejsonRedactor.Invoke("GetJson", jsonToRedact);
        }
        #endregion

        [TestMethod]
        public void RedactToken_ShouldCallProcessChildTokensAndRedactPropertyAndRedactValue()
        {
            using (ShimsContext.Create())
            {
                var jsonRedactor = new JsonRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateJsonRedactor = new PrivateObject(jsonRedactor);
                var calledProcessChildTokens = false;
                var calledRedactProperty = false;
                var calledRedactValue = false;
                ShimJsonRedactor.AllInstances.ProcessChildTokensJTokenBoolean = (x, y, z) => { calledProcessChildTokens = true; };
                ShimJsonRedactor.AllInstances.RedactPropertyJTokenBoolean = (x, y, z) => { calledRedactProperty = true; };
                ShimJsonRedactor.AllInstances.RedactValueJTokenBoolean = (x, y, z) => { calledRedactValue = true; };

                privateJsonRedactor.Invoke("RedactToken", null as JToken, false);

                Assert.IsTrue(calledProcessChildTokens);
                Assert.IsTrue(calledRedactProperty);
                Assert.IsTrue(calledRedactValue);
            }
        }

        #region ProcessChildTokens
        #endregion

        #region HasPiiParent
        #endregion

        #region RedactProperty
        #endregion

        #region TryGetPropertyAndValue
        #endregion

        #region TryGetProperty
        #endregion

        #region RedactValue
        #endregion
        #endregion
    }
}
