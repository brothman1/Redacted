using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Newtonsoft.Json.Linq;
using Redacted.Fakes;
using System.Linq;

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
        [TestMethod]
        public void ProcessChildTokens_WhenParentTokenIsJObjectOrJArrayAndRedactByNameIsTrue_ShouldCallRedactTokenOncePerChildAndNotCallHasPiiParent()
        {
            using (ShimsContext.Create())
            {
                var jsonRedactor = new JsonRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateJsonRedactor = new PrivateObject(jsonRedactor);
                var parentToken = new JObject
                {
                    { "Property1", new JValue("Value1") },
                    { "Property2", new JValue("Value2") }
                };
                var redactTokenCalls = 0;
                ShimJsonRedactor.AllInstances.RedactTokenJTokenBoolean = (x, y, z) => redactTokenCalls++;
                var hasPiiParentCalls = 0;
                ShimJsonRedactor.AllInstances.HasPiiParentJToken = (x, y) =>
                {
                    hasPiiParentCalls++;
                    return true;
                };

                privateJsonRedactor.Invoke("ProcessChildTokens", parentToken, true);

                Assert.AreEqual(parentToken.Children().Count(), redactTokenCalls);
                Assert.AreEqual(0, hasPiiParentCalls);
            }
        }

        [TestMethod]
        public void ProcessChildTokens_WhenParentTokenIsJObjectOrJArrayAndRedactByNameIsFalse_ShouldCallRedactTokenAndHasPiiParentOncePerChild()
        {
            using (ShimsContext.Create())
            {
                var jsonRedactor = new JsonRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateJsonRedactor = new PrivateObject(jsonRedactor);
                var parentToken = new JObject
                {
                    { "Property1", new JValue("Value1") },
                    { "Property2", new JValue("Value2") }
                };
                var redactTokenCalls = 0;
                ShimJsonRedactor.AllInstances.RedactTokenJTokenBoolean = (x, y, z) => redactTokenCalls++;
                var hasPiiParentCalls = 0;
                ShimJsonRedactor.AllInstances.HasPiiParentJToken = (x, y) =>
                {
                    hasPiiParentCalls++;
                    return true;
                };

                privateJsonRedactor.Invoke("ProcessChildTokens", parentToken, false);

                Assert.AreEqual(parentToken.Children().Count(), redactTokenCalls);
                Assert.AreEqual(parentToken.Children().Count(), hasPiiParentCalls);
            }
        }

        [TestMethod]
        public void ProcessChildTokens_WhenParentTokenIsNotJObjectOrJArray_ShouldNotCallRedactTokenOrHasPiiParent()
        {
            using (ShimsContext.Create())
            {
                var jsonRedactor = new JsonRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateJsonRedactor = new PrivateObject(jsonRedactor);
                var parentToken = new JValue("Value1");
                var redactTokenCalls = 0;
                ShimJsonRedactor.AllInstances.RedactTokenJTokenBoolean = (x, y, z) => redactTokenCalls++;
                var hasPiiParentCalls = 0;
                ShimJsonRedactor.AllInstances.HasPiiParentJToken = (x, y) =>
                {
                    hasPiiParentCalls++;
                    return true;
                };

                privateJsonRedactor.Invoke("ProcessChildTokens", parentToken, true);

                Assert.AreEqual(0, redactTokenCalls);
                Assert.AreEqual(0, hasPiiParentCalls);
            }
        }

        [TestMethod]
        public void ProcessChildTokens_WhenParentTokenIsNull_ShouldNotCallRedactTokenOrHasPiiParent()
        {
            using (ShimsContext.Create())
            {
                var jsonRedactor = new JsonRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateJsonRedactor = new PrivateObject(jsonRedactor);
                var parentToken = null as JToken;
                var redactTokenCalls = 0;
                ShimJsonRedactor.AllInstances.RedactTokenJTokenBoolean = (x, y, z) => redactTokenCalls++;
                var hasPiiParentCalls = 0;
                ShimJsonRedactor.AllInstances.HasPiiParentJToken = (x, y) =>
                {
                    hasPiiParentCalls++;
                    return true;
                };

                privateJsonRedactor.Invoke("ProcessChildTokens", parentToken, true);

                Assert.AreEqual(0, redactTokenCalls);
                Assert.AreEqual(0, hasPiiParentCalls);
            }
        }
        #endregion

        #region HasPiiParent
        [TestMethod]
        public void HasPiiParent_WhenTokenToRedactIsJArrayAndTokenToRedactParentIsJPropertyAndRedactNameIsTrueAndIsPiiNameReturnsTrue_ShouldReturnTrue()
        {
            using (ShimsContext.Create())
            {
                var jsonRedactor = new JsonRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateJsonRedactor = new PrivateObject(jsonRedactor);
                var parentToken = new JProperty("Property1", new JArray());
                var tokenToRedact = parentToken.First;
                ShimRedactor.AllInstances.RedactNameGet = (x) => true;
                ShimRedactor.AllInstances.IsPiiNameString = (x, y) => true;

                Assert.IsTrue((bool)privateJsonRedactor.Invoke("HasPiiParent", tokenToRedact));
            }
        }

        [TestMethod]
        public void HasPiiParent_WhenTokenToRedactIsJArrayAndTokenToRedactParentIsJPropertyAndRedactNameIsTrueAndIsPiiNameReturnsFalse_ShouldReturnFalse()
        {
            using (ShimsContext.Create())
            {
                var jsonRedactor = new JsonRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateJsonRedactor = new PrivateObject(jsonRedactor);
                var parentToken = new JProperty("Property1", new JArray());
                var tokenToRedact = parentToken.First;
                ShimRedactor.AllInstances.RedactNameGet = (x) => true;
                ShimRedactor.AllInstances.IsPiiNameString = (x, y) => false;

                Assert.IsFalse((bool)privateJsonRedactor.Invoke("HasPiiParent", tokenToRedact));
            }
        }

        [TestMethod]
        public void HasPiiParent_WhenTokenToRedactIsJArrayAndTokenToRedactParentIsJPropertyAndRedactNameIsFalse_ShouldReturnFalse()
        {
            using (ShimsContext.Create())
            {
                var jsonRedactor = new JsonRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateJsonRedactor = new PrivateObject(jsonRedactor);
                var parentToken = new JProperty("Property1", new JArray());
                var tokenToRedact = parentToken.First;
                ShimRedactor.AllInstances.RedactNameGet = (x) => false;

                Assert.IsFalse((bool)privateJsonRedactor.Invoke("HasPiiParent", tokenToRedact));
            }
        }

        [TestMethod]
        public void HasPiiParent_WhenTokenToRedactIsJArrayAndTokenToRedactParentIsNotJProperty_ShouldReturnFalse()
        {
            var jsonRedactor = new JsonRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
            var privateJsonRedactor = new PrivateObject(jsonRedactor);
            var parentToken = new JArray(new JArray());
            var tokenToRedact = parentToken.First;

            Assert.IsFalse((bool)privateJsonRedactor.Invoke("HasPiiParent", tokenToRedact));
        }

        [TestMethod]
        public void HasPiiParent_WhenTokenToRedactIsNotJArray_ShouldReturnFalse()
        {
            var jsonRedactor = new JsonRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
            var privateJsonRedactor = new PrivateObject(jsonRedactor);
            var tokenToRedact = new JValue("Value1");

            Assert.IsFalse((bool)privateJsonRedactor.Invoke("HasPiiParent", tokenToRedact));
        }

        [TestMethod]
        public void HasPiiParent_WhenTokenToRedactIsNull_ShouldReturnFalse()
        {
            var jsonRedactor = new JsonRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
            var privateJsonRedactor = new PrivateObject(jsonRedactor);
            var tokenToRedact = null as JToken;

            Assert.IsFalse((bool)privateJsonRedactor.Invoke("HasPiiParent", tokenToRedact));
        }
        #endregion

        #region RedactProperty
        [TestMethod]
        public void RedactProperty_WhenTryGetPropertyAndValueReturnsTrueAndRedactByNameIsTrue_ShouldCallRedactValueAndNotCallRedactNameOrIsPiiName()
        {
            using (ShimsContext.Create())
            {
                var jsonRedactor = new JsonRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateJsonRedactor = new PrivateObject(jsonRedactor);
                var tokenToRedact = new JProperty("Property1", new JValue("Value1"));
                ShimJsonRedactor.AllInstances.TryGetPropertyAndValueJTokenJPropertyOutJTokenOut = (JsonRedactor w, JToken x, out JProperty y, out JToken z) =>
                {
                    y = tokenToRedact;
                    z = tokenToRedact.Value;
                    return true;
                };
                var redactValueCalls = 0;
                ShimJsonRedactor.AllInstances.RedactValueJTokenBoolean = (x, y, z) => redactValueCalls++;
                var redactNameGetCalls = 0;
                ShimRedactor.AllInstances.RedactNameGet = (x) =>
                {
                    redactNameGetCalls++;
                    return true;
                };
                var isPiiNameCalls = 0;
                ShimRedactor.AllInstances.IsPiiNameString = (x, y) =>
                {
                    isPiiNameCalls++;
                    return true;
                };

                privateJsonRedactor.Invoke("RedactProperty", tokenToRedact, true);

                Assert.AreEqual(1, redactValueCalls);
                Assert.AreEqual(0, redactNameGetCalls);
                Assert.AreEqual(0, isPiiNameCalls);
            }
        }

        [TestMethod]
        public void RedactProperty_WhenTryGetPropertyAndValueReturnsTrueAndRedactByNameIsFalseAndRedactNameIsTrue_ShouldCallRedactValueAndRedactNameAndIsPiiName()
        {
            using (ShimsContext.Create())
            {
                var jsonRedactor = new JsonRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateJsonRedactor = new PrivateObject(jsonRedactor);
                var tokenToRedact = new JProperty("Property1", new JValue("Value1"));
                ShimJsonRedactor.AllInstances.TryGetPropertyAndValueJTokenJPropertyOutJTokenOut = (JsonRedactor w, JToken x, out JProperty y, out JToken z) =>
                {
                    y = tokenToRedact;
                    z = tokenToRedact.Value;
                    return true;
                };
                var redactValueCalls = 0;
                ShimJsonRedactor.AllInstances.RedactValueJTokenBoolean = (x, y, z) => redactValueCalls++;
                var redactNameGetCalls = 0;
                ShimRedactor.AllInstances.RedactNameGet = (x) =>
                {
                    redactNameGetCalls++;
                    return true;
                };
                var isPiiNameCalls = 0;
                ShimRedactor.AllInstances.IsPiiNameString = (x, y) =>
                {
                    isPiiNameCalls++;
                    return true;
                };

                privateJsonRedactor.Invoke("RedactProperty", tokenToRedact, false);

                Assert.AreEqual(1, redactValueCalls);
                Assert.AreEqual(1, redactNameGetCalls);
                Assert.AreEqual(1, isPiiNameCalls);
            }
        }

        [TestMethod]
        public void RedactProperty_WhenTryGetPropertyAndValueReturnsTrueAndRedactByNameIsFalseAndRedactNameIsFalse_ShouldCallRedactValueAndRedactNameAndNotCallIsPiiName()
        {
            using (ShimsContext.Create())
            {
                var jsonRedactor = new JsonRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateJsonRedactor = new PrivateObject(jsonRedactor);
                var tokenToRedact = new JProperty("Property1", new JValue("Value1"));
                ShimJsonRedactor.AllInstances.TryGetPropertyAndValueJTokenJPropertyOutJTokenOut = (JsonRedactor w, JToken x, out JProperty y, out JToken z) =>
                {
                    y = tokenToRedact;
                    z = tokenToRedact.Value;
                    return true;
                };
                var redactValueCalls = 0;
                ShimJsonRedactor.AllInstances.RedactValueJTokenBoolean = (x, y, z) => redactValueCalls++;
                var redactNameGetCalls = 0;
                ShimRedactor.AllInstances.RedactNameGet = (x) =>
                {
                    redactNameGetCalls++;
                    return false;
                };
                var isPiiNameCalls = 0;
                ShimRedactor.AllInstances.IsPiiNameString = (x, y) =>
                {
                    isPiiNameCalls++;
                    return true;
                };

                privateJsonRedactor.Invoke("RedactProperty", tokenToRedact, false);

                Assert.AreEqual(1, redactValueCalls);
                Assert.AreEqual(1, redactNameGetCalls);
                Assert.AreEqual(0, isPiiNameCalls);
            }
        }

        [TestMethod]
        public void RedactProperty_WhenTryGetPropertyAndValueReturnsFalseAndOutsNonNullPropertyAndValue_ShouldCallRedactTokenAndNotCallRedactValueAndRedactNameAndIsPiiName()
        {
            using (ShimsContext.Create())
            {
                var jsonRedactor = new JsonRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateJsonRedactor = new PrivateObject(jsonRedactor);
                var tokenToRedact = new JProperty("Property1", new JValue("Value1"));
                ShimJsonRedactor.AllInstances.TryGetPropertyAndValueJTokenJPropertyOutJTokenOut = (JsonRedactor w, JToken x, out JProperty y, out JToken z) =>
                {
                    y = tokenToRedact;
                    z = tokenToRedact.Value;
                    return false;
                };
                var redactValueCalls = 0;
                ShimJsonRedactor.AllInstances.RedactValueJTokenBoolean = (x, y, z) => redactValueCalls++;
                var redactNameGetCalls = 0;
                ShimRedactor.AllInstances.RedactNameGet = (x) =>
                {
                    redactNameGetCalls++;
                    return true;
                };
                var isPiiNameCalls = 0;
                ShimRedactor.AllInstances.IsPiiNameString = (x, y) =>
                {
                    isPiiNameCalls++;
                    return true;
                };
                var redactTokenCalls = 0;
                ShimJsonRedactor.AllInstances.RedactTokenJTokenBoolean = (x, y, z) => redactTokenCalls++;

                privateJsonRedactor.Invoke("RedactProperty", tokenToRedact, true);

                Assert.AreEqual(0, redactValueCalls);
                Assert.AreEqual(0, redactNameGetCalls);
                Assert.AreEqual(0, isPiiNameCalls);
                Assert.AreEqual(1, redactTokenCalls);
            }
        }
        #endregion

        #region TryGetPropertyAndValue
        [TestMethod]
        public void TryGetPropertyAndValue_WhenTryGetPropertyReturnsTrueAndOutputsPropertyWithJValue_ShouldOutPropertyAndValueAndReturnTrue()
        {
            using (ShimsContext.Create())
            {
                var jsonRedactor = new JsonRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateJsonRedactor = new PrivateObject(jsonRedactor);
                var tokenToRedact = new JProperty("Property1", new JValue("Value1"));
                ShimJsonRedactor.AllInstances.TryGetPropertyJTokenJPropertyOut = (JsonRedactor x, JToken y, out JProperty z) =>
                {
                    z = tokenToRedact;
                    return true;
                };
                var parameters = new object[] { tokenToRedact, new JProperty("",""), new JValue("") };

                var returnValue = (bool)privateJsonRedactor.Invoke("TryGetPropertyAndValue", parameters);

                Assert.IsTrue(returnValue);
                Assert.AreEqual(tokenToRedact, parameters[1]);
                Assert.AreEqual(tokenToRedact.Value, parameters[2]);
            }
        }

        [TestMethod]
        public void TryGetPropertyAndValue_WhenTryGetPropertyReturnsTrueAndDoesNotOutputPropertyWithJValueAndPropertyIsNotNull_ShouldOutPropertyAndValueAndReturnFalse()
        {
            using (ShimsContext.Create())
            {
                var jsonRedactor = new JsonRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateJsonRedactor = new PrivateObject(jsonRedactor);
                var tokenToRedact = new JProperty("Property1", new JArray());
                ShimJsonRedactor.AllInstances.TryGetPropertyJTokenJPropertyOut = (JsonRedactor x, JToken y, out JProperty z) =>
                {
                    z = tokenToRedact;
                    return true;
                };
                var parameters = new object[] { tokenToRedact, new JProperty("", ""), new JValue("") };

                var returnValue = (bool)privateJsonRedactor.Invoke("TryGetPropertyAndValue", parameters);

                Assert.IsFalse(returnValue);
                Assert.AreEqual(tokenToRedact, parameters[1]);
                Assert.AreEqual(tokenToRedact.Value, parameters[2]);
            }
        }

        [TestMethod]
        public void TryGetPropertyAndValue_WhenTryGetPropertyReturnsFalse_ShouldOutNullPropertyAndValueAndReturnFalse()
        {
            using (ShimsContext.Create())
            {
                var jsonRedactor = new JsonRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateJsonRedactor = new PrivateObject(jsonRedactor);
                var tokenToRedact = new JProperty("Property1", new JValue("Value1"));
                ShimJsonRedactor.AllInstances.TryGetPropertyJTokenJPropertyOut = (JsonRedactor x, JToken y, out JProperty z) =>
                {
                    z = null;
                    return false;
                };
                var parameters = new object[] { tokenToRedact, new JProperty("", ""), new JValue("") };

                var returnValue = (bool)privateJsonRedactor.Invoke("TryGetPropertyAndValue", parameters);

                Assert.IsFalse(returnValue);
                Assert.AreEqual(null, parameters[1]);
                Assert.AreEqual(null, parameters[2]);
            }
        }
        #endregion

        #region TryGetProperty
        [TestMethod]
        public void TryGetProperty_WhenTokenToRedactIsJProperty_ShouldOutPropertyAndReturnTrue()
        {
            var jsonRedactor = new JsonRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
            var privateJsonRedactor = new PrivateObject(jsonRedactor);
            var tokenToRedact = new JProperty("Property1", new JValue("Value1"));
            var parameters = new object[] { tokenToRedact, new JProperty("", "") };

            var returnValue = (bool)privateJsonRedactor.Invoke("TryGetProperty", parameters);

            Assert.IsTrue(returnValue);
            Assert.AreEqual(tokenToRedact, parameters[1]);
        }

        [TestMethod]
        public void TryGetProperty_WhenTokenToRedactIsNotJProperty_ShouldOutNullAndReturnFalse()
        {
            var jsonRedactor = new JsonRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
            var privateJsonRedactor = new PrivateObject(jsonRedactor);
            var tokenToRedact = new JValue("SomeValue");
            var parameters = new object[] { tokenToRedact, new JProperty("", "") };

            var returnValue = (bool)privateJsonRedactor.Invoke("TryGetProperty", parameters);

            Assert.IsFalse(returnValue);
            Assert.AreEqual(null, parameters[1]);
        }
        #endregion

        #region RedactValue
        [TestMethod]
        public void RedactValue_WhenTokenToRedactIsJValueAndJValueIsString_ShouldCallGetRedactedValueAndUpdateValue()
        {
            using (ShimsContext.Create())
            {
                var jsonRedactor = new JsonRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateJsonRedactor = new PrivateObject(jsonRedactor);
                var tokenToRedact = new JValue("SomeValue");
                var redactedValue = "RedactedValue";
                var getRedactedValueCalls = 0;
                ShimRedactor.AllInstances.GetRedactedValueStringBoolean = (x, y, z) =>
                {
                    getRedactedValueCalls++;
                    return redactedValue;
                };

                privateJsonRedactor.Invoke("RedactValue", tokenToRedact, false);

                Assert.AreEqual(1, getRedactedValueCalls);
                Assert.AreEqual(redactedValue, tokenToRedact.Value);
            }
        }

        [TestMethod]
        public void RedactValue_WhenTokenToRedactIsJValueAndJValueIsNotString_ShouldNotCallGetRedactedValueOrUpdateValue()
        {
            using (ShimsContext.Create())
            {
                var jsonRedactor = new JsonRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateJsonRedactor = new PrivateObject(jsonRedactor);
                var tokenToRedact = new JValue(0);
                var redactedValue = "RedactedValue";
                var getRedactedValueCalls = 0;
                ShimRedactor.AllInstances.GetRedactedValueStringBoolean = (x, y, z) =>
                {
                    getRedactedValueCalls++;
                    return redactedValue;
                };

                privateJsonRedactor.Invoke("RedactValue", tokenToRedact, false);

                Assert.AreEqual(0, getRedactedValueCalls);
                Assert.AreNotEqual(redactedValue, tokenToRedact.Value);
            }
        }

        [TestMethod]
        public void RedactValue_WhenTokenToRedactIsNotJValue_ShouldNotCallGetRedactedValueOrUpdateValue()
        {
            using (ShimsContext.Create())
            {
                var jsonRedactor = new JsonRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateJsonRedactor = new PrivateObject(jsonRedactor);
                var tokenToRedact = new JProperty("Property1", "SomeValue");
                var redactedValue = "RedactedValue";
                var getRedactedValueCalls = 0;
                ShimRedactor.AllInstances.GetRedactedValueStringBoolean = (x, y, z) =>
                {
                    getRedactedValueCalls++;
                    return redactedValue;
                };

                privateJsonRedactor.Invoke("RedactValue", tokenToRedact, false);

                Assert.AreEqual(0, getRedactedValueCalls);
                Assert.AreNotEqual(redactedValue, tokenToRedact.Value);
            }
        }
        #endregion
        #endregion
    }
}
