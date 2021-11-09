using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Newtonsoft.Json;
using Moq;
using Redacted;
using Redacted.Configuration;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.QualityTools.Testing.Fakes;
using Redacted.Fakes;
using System.Linq;

namespace Redacted.Tests
{
    [TestClass]
    public class RedactorTests
    {
        private static PrivateType _privateRedactorType;

        [ClassInitialize]
        public static void ClassInitializer(TestContext context)
        {
            _privateRedactorType = new PrivateType(typeof(Redactor));
        }

        #region Properties
        [TestMethod]
        public void RedactorCollection_Get_WhenRedactorHasBeenInitializedXTimes_ShouldReturnIEnumerableWithXRedactors()
        {
            var currentRedactorCount = Redactor.RedactorCollection.Count();
            _ = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
            _ = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));

            Assert.AreEqual(currentRedactorCount + 2, Redactor.RedactorCollection.Count());
        }

        [TestMethod]
        public void Id_Get_WhenRedactorHasBeenInitializedXTimes_ShouldReturnXPlusOneForNewRedactor()
        {
            var currentRedactorCount = Redactor.RedactorCollection.Count();
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));

            Assert.AreEqual(currentRedactorCount + 1, redactor.Id);
        }

        [TestMethod]
        public void RedactorType_Get_WhenRedactorIsNotXmlOrJsonRedactor_ShouldReturnRedactorTypeNone()
        {
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));

            Assert.AreEqual(RedactorType.None, redactor.RedactorType);
        }

        [TestMethod]
        public void RedactNames_Get_WhenRedactorHasBeenInitialized_ShouldReturnRedactNames()
        {
            var config = RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern);
            var redactor = new Redactor(config);

            Assert.AreEqual(config.RedactNames, redactor.RedactNames);
        }

        [TestMethod]
        public void NameRedactValue_Get_WhenRedactorHasBeenInitialized_ShouldReturnNameRedactValue()
        {
            var config = RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern);
            var redactor = new Redactor(config);

            Assert.AreEqual(config.NameRedactValue, redactor.NameRedactValue);
        }

        [TestMethod]
        public void RedactPatterns_Get_WhenRedactorHasBeenInitialized_ShouldReturnRedactPatterns()
        {
            var config = RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern);
            var redactor = new Redactor(config);

            Assert.AreEqual(config.RedactPatterns, redactor.RedactPatterns);
        }

        [TestMethod]
        public void RedactBy_Get_WhenRedactorHasBeenInitialized_ShouldReturnRedactBy()
        {
            var config = RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern);
            var redactor = new Redactor(config);

            Assert.AreEqual(config.RedactBy, redactor.RedactBy);
        }

        #region RedactName_Get
        [TestMethod]
        public void RedactName_Get_WhenRedactByIsNameAndPatternOrName_ShouldReturnTrue()
        {
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));

            Assert.IsTrue(redactor.RedactName);
        }

        [TestMethod]
        public void RedactName_Get_WhenRedactByIsNotNameAndPatternOrName_ShouldReturnFalse()
        {
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.Pattern));

            Assert.IsFalse(redactor.RedactName);
        }
        #endregion

        #region RedactPattern
        [TestMethod]
        public void RedactPattern_Get_WhenRedactByIsNameAndPatternOrPattern_ShouldReturnTrue()
        {
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));

            Assert.IsTrue(redactor.RedactPattern);
        }

        [TestMethod]
        public void RedactPattern_Get_WhenRedactByIsNotNameAndPatternOrPattern_ShouldReturnFalse()
        {
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.Name));

            Assert.IsFalse(redactor.RedactPattern);
        }
        #endregion


        [TestMethod]
        public void ParentRedactor_Get_WhenRedactorHasBeenInitialized_ShouldReturnParentRedactor()
        {
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));

            Assert.AreEqual(null, redactor.ParentRedactor);
        }

        #region XmlRedactor_Get
        [TestMethod]
        public void XmlRedactor_Get_WhenXmlRedactorHasBeenInitialized_ReturnXmlRedactor()
        {
            var config = RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern);
            var redactor = new Redactor(config);
            var privateRedactor = new PrivateObject(redactor);
            privateRedactor.SetField("_xmlRedactor", redactor);

            Assert.AreEqual(redactor, redactor.XmlRedactor);
        }

        [TestMethod]
        public void XmlRedactor_Get_WhenXmlRedactorHasNotBeenInitializedAndParentRedactorHasXmlRedactor_ShouldReturnParentXmlRedactor()
        {
            var config = RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern);
            var parentRedactor = new Redactor(config);
            var privateParentRedactor = new PrivateObject(parentRedactor);
            privateParentRedactor.SetField("_xmlRedactor", parentRedactor);
            var redactor = new Redactor(config);
            var privateRedactor = new PrivateObject(redactor);
            privateRedactor.SetProperty("ParentRedactor", parentRedactor);

            Assert.AreEqual(parentRedactor, redactor.XmlRedactor);
        }

        [TestMethod]
        public void XmlRedactor_Get_WhenXmlRedactorHasNotBeenInitializedAndParentRedactorDoesNotHaveXmlRedactor_ShouldCallBuildXmlRedactor()
        {
            using (ShimsContext.Create())
            {
                var config = RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern);
                var redactor = new Redactor(config);
                var called = false;
                ShimRedactor.AllInstances.BuildXmlRedactor = (x) => 
                { 
                    called = true;
                    return null;
                };

                _ = redactor.XmlRedactor;

                Assert.IsTrue(called);
            }
        }
        #endregion

        #region JsonRedactor_Get
        [TestMethod]
        public void JsonRedactor_Get_WhenJsonRedactorHasBeenInitialized_ReturnJsonRedactor()
        {
            var config = RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern);
            var redactor = new Redactor(config);
            var privateRedactor = new PrivateObject(redactor);
            privateRedactor.SetField("_jsonRedactor", redactor);

            Assert.AreEqual(redactor, redactor.JsonRedactor);
        }

        [TestMethod]
        public void JsonRedactor_Get_WhenJsonRedactorHasNotBeenInitializedAndParentRedactorHasJsonRedactor_ShouldReturnParentJsonRedactor()
        {
            var config = RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern);
            var parentRedactor = new Redactor(config);
            var privateParentRedactor = new PrivateObject(parentRedactor);
            privateParentRedactor.SetField("_jsonRedactor", parentRedactor);
            var redactor = new Redactor(config);
            var privateRedactor = new PrivateObject(redactor);
            privateRedactor.SetProperty("ParentRedactor", parentRedactor);

            Assert.AreEqual(parentRedactor, redactor.JsonRedactor);
        }

        [TestMethod]
        public void JsonRedactor_Get_WhenJsonRedactorHasNotBeenInitializedAndParentRedactorDoesNotHaveJsonRedactor_ShouldCallBuildJsonRedactor()
        {
            using (ShimsContext.Create())
            {
                var config = RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern);
                var redactor = new Redactor(config);
                var called = false;
                ShimRedactor.AllInstances.BuildJsonRedactor = (x) =>
                {
                    called = true;
                    return null;
                };

                _ = redactor.JsonRedactor;

                Assert.IsTrue(called);
            }
        }
        #endregion
        #endregion

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

        #region Methods
        #region Redact
        [TestMethod]
        public void Redact_WhenValueToRedactHasXmlEdgesAndIsValidXml_ShouldReturnRedactedXmlValue()
        {
            var valueToRedact = RedactedResource.Get("Redactor", "xml");
            var expectedValue = RedactedResource.Get("Redactor_Redacted", "xml");
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));

            var returnValue = redactor.Redact(valueToRedact);

            Assert.AreEqual(expectedValue.RemoveWhitespace(), returnValue.RemoveWhitespace());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Redact_WhenValueToRedactHasXmlEdgesAndIsNotValidXml_ShouldThrowArgumentException()
        {
            var valueToRedact = "<Not XML>>";
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.Name));

            _ = redactor.Redact(valueToRedact);
        }

        [TestMethod]
        public void Redact_WhenValueToRedactHasJsonEdgesAndIsValidJson_ShouldReturnRedactedJsonValue()
        {
            var valueToRedact = RedactedResource.Get("Redactor");
            var expectedValue = RedactedResource.Get("Redactor_Redacted");
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));

            var returnValue = redactor.Redact(valueToRedact);

            Assert.AreEqual(expectedValue.RemoveWhitespace(), returnValue.RemoveWhitespace());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Redact_WhenValueToRedactHasJsonEdgesAndIsNotValidJson_ShouldThrowArgumentException()
        {
            var valueToRedact = "{Not JSON}}";
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.Name));

            _ = redactor.Redact(valueToRedact);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Redact_WhenValueToRedactDoesNotHaveXmlOrJsonEdges_ShouldThrowArgumentException()
        {
            var valueToRedact = "Not XML or JSON";
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.Name));

            _ = redactor.Redact(valueToRedact);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Redact_WhenValueToRedactIsNull_ShouldThrowArgumentException()
        {
            var valueToRedact = null as string;
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.Name));

            _ = redactor.Redact(valueToRedact);
        }
        #endregion

        #region HasXmlEdges
        [TestMethod]
        public void HasXmlEdges_WhenHasXmlStartAndHasXmlEndReturnTrue_ShouldReturnTrue()
        {
            using (ShimsContext.Create())
            {
                var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateRedactor = new PrivateObject(redactor);
                ShimRedactor.AllInstances.HasXmlStartString = (x, y) => { return true; };
                ShimRedactor.AllInstances.HasXmlEndString = (x, y) => { return true; };

                Assert.IsTrue((bool)privateRedactor.Invoke("HasXmlEdges", "<This has XML edges.>"));
            }
        }

        [TestMethod]
        public void HasXmlEdges_WhenHasXmlStartReturnsTrueAndHasXmlEndReturnFalse_ShouldReturnFalse()
        {
            using (ShimsContext.Create())
            {
                var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateRedactor = new PrivateObject(redactor);
                ShimRedactor.AllInstances.HasXmlStartString = (x, y) => { return true; };
                ShimRedactor.AllInstances.HasXmlEndString = (x, y) => { return false; };

                Assert.IsFalse((bool)privateRedactor.Invoke("HasXmlEdges", "<This has XML start."));
            }
        }

        [TestMethod]
        public void HasXmlEdges_WhenXmlValueHasXmlStartReturnsFalseAndHasXmlEndReturnTrue_ShouldReturnFalse()
        {
            using (ShimsContext.Create())
            {
                var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateRedactor = new PrivateObject(redactor);
                ShimRedactor.AllInstances.HasXmlStartString = (x, y) => { return false; };
                ShimRedactor.AllInstances.HasXmlEndString = (x, y) => { return true; };

                Assert.IsFalse((bool)privateRedactor.Invoke("HasXmlEdges", "This has XML end.>"));
            }
        }

        [TestMethod]
        public void HasXmlEdges_WhenXmlValueHasXmlStartReturnsFalseAndHasXmlEndReturnFalse_ShouldReturnFalse()
        {
            using (ShimsContext.Create())
            {
                var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateRedactor = new PrivateObject(redactor);
                ShimRedactor.AllInstances.HasXmlStartString = (x, y) => { return false; };
                ShimRedactor.AllInstances.HasXmlEndString = (x, y) => { return false; };

                Assert.IsFalse((bool)privateRedactor.Invoke("HasXmlEdges", "This has no XML edges."));
            }
        }
        #endregion

        #region HasXmlStart
        [TestMethod]
        public void HasXmlStart_WhenXmlValueIsNotNullAndStartsWithXmlStart_ShouldReturnTrue()
        {
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
            var privateRedactor = new PrivateObject(redactor);
            var xmlStart = _privateRedactorType.GetStaticFieldOrProperty("XmlStart") as string;
            var xmlValue = $"{xmlStart}This has XML start.";

            Assert.IsTrue((bool)privateRedactor.Invoke("HasXmlStart", xmlValue));
        }

        [TestMethod]
        public void HasXmlStart_WhenXmlValueIsNotNullAndDoesNotStartWithXmlStart_ShouldReturnFalse()
        {
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
            var privateRedactor = new PrivateObject(redactor);
            var xmlValue = "This does not have XML start.";

            Assert.IsFalse((bool)privateRedactor.Invoke("HasXmlStart", xmlValue));
        }

        [TestMethod]
        public void HasXmlStart_WhenXmlValueIsNull_ShouldReturnFalse()
        {
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
            var privateRedactor = new PrivateObject(redactor);
            var xmlValue = null as string;

            Assert.IsFalse((bool)privateRedactor.Invoke("HasXmlStart", xmlValue));
        }
        #endregion

        #region HasXmlEnd
        [TestMethod]
        public void HasXmlEnd_WhenXmlValueIsNotNullAndEndsWithXmlEnd_ShouldReturnTrue()
        {
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
            var privateRedactor = new PrivateObject(redactor);
            var xmlEnd = _privateRedactorType.GetStaticFieldOrProperty("XmlEnd") as string;
            var xmlValue = $"This has XML end.{xmlEnd}";

            Assert.IsTrue((bool)privateRedactor.Invoke("HasXmlEnd", xmlValue));
        }

        [TestMethod]
        public void HasXmlEnd_WhenXmlValueIsNotNullAndDoesNotEndWithXmlEnd_ShouldReturnFalse()
        {
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
            var privateRedactor = new PrivateObject(redactor);
            var xmlValue = "This does not have XML end.";

            Assert.IsFalse((bool)privateRedactor.Invoke("HasXmlStart", xmlValue));
        }

        [TestMethod]
        public void HasXmlEnd_WhenXmlValueIsNull_ShouldReturnFalse()
        {
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
            var privateRedactor = new PrivateObject(redactor);
            var xmlValue = null as string;

            Assert.IsFalse((bool)privateRedactor.Invoke("HasXmlEnd", xmlValue));
        }
        #endregion

        #region BuildXmlRedactor
        [TestMethod]
        public void BuildXmlRedactor_WhenRedactByIsNameOrPatternOrNameAndPattern_ShouldReturnXmlRedactorWithParent()
        {
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
            var privateRedactor = new PrivateObject(redactor);

            var returnValue = privateRedactor.Invoke("BuildXmlRedactor") as IRedactor;

            Assert.AreEqual(typeof(XmlRedactor), returnValue.GetType());
            Assert.AreEqual(redactor, returnValue.ParentRedactor as Redactor);

        }

        [TestMethod]
        public void BuildXmlRedactor_WhenRedactByIsNotNameOrPatternOrNameAndPattern_ShouldReturnNull()
        {
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.None));
            var privateRedactor = new PrivateObject(redactor);

            var returnValue = privateRedactor.Invoke("BuildXmlRedactor") as IRedactor;

            Assert.AreEqual(null, returnValue);
        }
        #endregion

        #region HasJsonEdges
        [TestMethod]
        public void HasJsonEdges_WhenHasJsonObjectEdgesReturnTrue_ShouldReturnTrue()
        {
            using (ShimsContext.Create())
            {
                var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateRedactor = new PrivateObject(redactor);
                ShimRedactor.AllInstances.HasJsonObjectEdgesString = (x, y) => { return true; };
                ShimRedactor.AllInstances.HasJsonArrayEdgesString = (x, y) => { return false; };

                Assert.IsTrue((bool)privateRedactor.Invoke("HasJsonEdges", "{This has Json edges.}"));
            }
        }

        [TestMethod]
        public void HasJsonEdges_WhenHasJsonArrayEdgesReturnTrue_ShouldReturnTrue()
        {
            using (ShimsContext.Create())
            {
                var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateRedactor = new PrivateObject(redactor);
                ShimRedactor.AllInstances.HasJsonObjectEdgesString = (x, y) => { return false; };
                ShimRedactor.AllInstances.HasJsonArrayEdgesString = (x, y) => { return true; };

                Assert.IsTrue((bool)privateRedactor.Invoke("HasJsonEdges", "[This has Json edges.]"));
            }
        }

        [TestMethod]
        public void HasJsonEdges_WhenHasJsonObjectEdgesReturnFalseAndHasJsonArrayEdgesReturnsFalse_ShouldReturnFalse()
        {
            using (ShimsContext.Create())
            {
                var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateRedactor = new PrivateObject(redactor);
                ShimRedactor.AllInstances.HasJsonObjectEdgesString = (x, y) => { return false; };
                ShimRedactor.AllInstances.HasJsonArrayEdgesString = (x, y) => { return false; };

                Assert.IsFalse((bool)privateRedactor.Invoke("HasJsonEdges", "This does not have Json edges.}"));
            }
        }
        #endregion

        #region HasJsonObjectEdges
        [TestMethod]
        public void HasJsonObjectEdges_WhenHasJsonObjectStartAndHasJsonObjectEndReturnTrue_ShouldReturnTrue()
        {
            using (ShimsContext.Create())
            {
                var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateRedactor = new PrivateObject(redactor);
                ShimRedactor.AllInstances.HasJsonObjectStartString = (x, y) => { return true; };
                ShimRedactor.AllInstances.HasJsonObjectEndString = (x, y) => { return true; };

                Assert.IsTrue((bool)privateRedactor.Invoke("HasJsonObjectEdges", "{This has Json Object edges.}"));
            }
        }

        [TestMethod]
        public void HasJsonObjectEdges_WhenHasJsonObjectStartReturnsTrueAndHasJsonObjectEndReturnFalse_ShouldReturnFalse()
        {
            using (ShimsContext.Create())
            {
                var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateRedactor = new PrivateObject(redactor);
                ShimRedactor.AllInstances.HasJsonObjectStartString = (x, y) => { return true; };
                ShimRedactor.AllInstances.HasJsonObjectEndString = (x, y) => { return false; };

                Assert.IsFalse((bool)privateRedactor.Invoke("HasJsonObjectEdges", "{This has Json Object start."));
            }
        }

        [TestMethod]
        public void HasJsonObjectEdges_WhenJsonObjectValueHasJsonObjectStartReturnsFalseAndHasJsonObjectEndReturnTrue_ShouldReturnFalse()
        {
            using (ShimsContext.Create())
            {
                var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateRedactor = new PrivateObject(redactor);
                ShimRedactor.AllInstances.HasJsonObjectStartString = (x, y) => { return false; };
                ShimRedactor.AllInstances.HasJsonObjectEndString = (x, y) => { return true; };

                Assert.IsFalse((bool)privateRedactor.Invoke("HasJsonObjectEdges", "This has Json Object end.}"));
            }
        }

        [TestMethod]
        public void HasJsonObjectEdges_WhenJsonObjectValueHasJsonObjectStartReturnsFalseAndHasJsonObjectEndReturnFalse_ShouldReturnFalse()
        {
            using (ShimsContext.Create())
            {
                var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateRedactor = new PrivateObject(redactor);
                ShimRedactor.AllInstances.HasJsonObjectStartString = (x, y) => { return false; };
                ShimRedactor.AllInstances.HasJsonObjectEndString = (x, y) => { return false; };

                Assert.IsFalse((bool)privateRedactor.Invoke("HasJsonObjectEdges", "This has no Json Object edges."));
            }
        }
        #endregion

        #region HasJsonObjectStart
        [TestMethod]
        public void HasJsonObjectStart_WhenJsonObjectValueIsNotNullAndStartsWithJsonObjectStart_ShouldReturnTrue()
        {
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
            var privateRedactor = new PrivateObject(redactor);
            var JsonObjectStart = _privateRedactorType.GetStaticFieldOrProperty("JsonObjectStart") as string;
            var JsonObjectValue = $"{JsonObjectStart}This has Json Object start.";

            Assert.IsTrue((bool)privateRedactor.Invoke("HasJsonObjectStart", JsonObjectValue));
        }

        [TestMethod]
        public void HasJsonObjectStart_WhenJsonObjectValueIsNotNullAndDoesNotStartWithJsonObjectStart_ShouldReturnFalse()
        {
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
            var privateRedactor = new PrivateObject(redactor);
            var JsonObjectValue = "This does not have Json Object start.";

            Assert.IsFalse((bool)privateRedactor.Invoke("HasJsonObjectStart", JsonObjectValue));
        }

        [TestMethod]
        public void HasJsonObjectStart_WhenJsonObjectValueIsNull_ShouldReturnFalse()
        {
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
            var privateRedactor = new PrivateObject(redactor);
            var JsonObjectValue = null as string;

            Assert.IsFalse((bool)privateRedactor.Invoke("HasJsonObjectStart", JsonObjectValue));
        }
        #endregion

        #region HasJsonObjectEnd
        [TestMethod]
        public void HasJsonObjectEnd_WhenJsonObjectValueIsNotNullAndEndsWithJsonObjectEnd_ShouldReturnTrue()
        {
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
            var privateRedactor = new PrivateObject(redactor);
            var JsonObjectEnd = _privateRedactorType.GetStaticFieldOrProperty("JsonObjectEnd") as string;
            var JsonObjectValue = $"This has Json Object end.{JsonObjectEnd}";

            Assert.IsTrue((bool)privateRedactor.Invoke("HasJsonObjectEnd", JsonObjectValue));
        }

        [TestMethod]
        public void HasJsonObjectEnd_WhenJsonObjectValueIsNotNullAndDoesNotEndWithJsonObjectEnd_ShouldReturnFalse()
        {
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
            var privateRedactor = new PrivateObject(redactor);
            var JsonObjectValue = "This does not have Json Object end.";

            Assert.IsFalse((bool)privateRedactor.Invoke("HasJsonObjectStart", JsonObjectValue));
        }

        [TestMethod]
        public void HasJsonObjectEnd_WhenJsonObjectValueIsNull_ShouldReturnFalse()
        {
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
            var privateRedactor = new PrivateObject(redactor);
            var JsonObjectValue = null as string;

            Assert.IsFalse((bool)privateRedactor.Invoke("HasJsonObjectEnd", JsonObjectValue));
        }
        #endregion

        #region HasJsonArrayEdges
        [TestMethod]
        public void HasJsonArrayEdges_WhenHasJsonArrayStartAndHasJsonArrayEndReturnTrue_ShouldReturnTrue()
        {
            using (ShimsContext.Create())
            {
                var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateRedactor = new PrivateObject(redactor);
                ShimRedactor.AllInstances.HasJsonArrayStartString = (x, y) => { return true; };
                ShimRedactor.AllInstances.HasJsonArrayEndString = (x, y) => { return true; };

                Assert.IsTrue((bool)privateRedactor.Invoke("HasJsonArrayEdges", "[This has Json Array edges.]"));
            }
        }

        [TestMethod]
        public void HasJsonArrayEdges_WhenHasJsonArrayStartReturnsTrueAndHasJsonArrayEndReturnFalse_ShouldReturnFalse()
        {
            using (ShimsContext.Create())
            {
                var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateRedactor = new PrivateObject(redactor);
                ShimRedactor.AllInstances.HasJsonArrayStartString = (x, y) => { return true; };
                ShimRedactor.AllInstances.HasJsonArrayEndString = (x, y) => { return false; };

                Assert.IsFalse((bool)privateRedactor.Invoke("HasJsonArrayEdges", "[This has Json Array start."));
            }
        }

        [TestMethod]
        public void HasJsonArrayEdges_WhenJsonArrayValueHasJsonArrayStartReturnsFalseAndHasJsonArrayEndReturnTrue_ShouldReturnFalse()
        {
            using (ShimsContext.Create())
            {
                var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateRedactor = new PrivateObject(redactor);
                ShimRedactor.AllInstances.HasJsonArrayStartString = (x, y) => { return false; };
                ShimRedactor.AllInstances.HasJsonArrayEndString = (x, y) => { return true; };

                Assert.IsFalse((bool)privateRedactor.Invoke("HasJsonArrayEdges", "This has Json Array end.]"));
            }
        }

        [TestMethod]
        public void HasJsonArrayEdges_WhenJsonArrayValueHasJsonArrayStartReturnsFalseAndHasJsonArrayEndReturnFalse_ShouldReturnFalse()
        {
            using (ShimsContext.Create())
            {
                var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateRedactor = new PrivateObject(redactor);
                ShimRedactor.AllInstances.HasJsonArrayStartString = (x, y) => { return false; };
                ShimRedactor.AllInstances.HasJsonArrayEndString = (x, y) => { return false; };

                Assert.IsFalse((bool)privateRedactor.Invoke("HasJsonArrayEdges", "This has no Json Array edges."));
            }
        }
        #endregion

        #region HasJsonArrayStart
        [TestMethod]
        public void HasJsonArrayStart_WhenJsonArrayValueIsNotNullAndStartsWithJsonArrayStart_ShouldReturnTrue()
        {
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
            var privateRedactor = new PrivateObject(redactor);
            var JsonArrayStart = _privateRedactorType.GetStaticFieldOrProperty("JsonArrayStart") as string;
            var JsonArrayValue = $"{JsonArrayStart}This has Json Array start.";

            Assert.IsTrue((bool)privateRedactor.Invoke("HasJsonArrayStart", JsonArrayValue));
        }

        [TestMethod]
        public void HasJsonArrayStart_WhenJsonArrayValueIsNotNullAndDoesNotStartWithJsonArrayStart_ShouldReturnFalse()
        {
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
            var privateRedactor = new PrivateObject(redactor);
            var JsonArrayValue = "This does not have Json Array start.";

            Assert.IsFalse((bool)privateRedactor.Invoke("HasJsonArrayStart", JsonArrayValue));
        }

        [TestMethod]
        public void HasJsonArrayStart_WhenJsonArrayValueIsNull_ShouldReturnFalse()
        {
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
            var privateRedactor = new PrivateObject(redactor);
            var JsonArrayValue = null as string;

            Assert.IsFalse((bool)privateRedactor.Invoke("HasJsonArrayStart", JsonArrayValue));
        }
        #endregion

        #region HasJsonArrayEnd
        [TestMethod]
        public void HasJsonArrayEnd_WhenJsonArrayValueIsNotNullAndEndsWithJsonArrayEnd_ShouldReturnTrue()
        {
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
            var privateRedactor = new PrivateObject(redactor);
            var JsonArrayEnd = _privateRedactorType.GetStaticFieldOrProperty("JsonArrayEnd") as string;
            var JsonArrayValue = $"This has Json Array end.{JsonArrayEnd}";

            Assert.IsTrue((bool)privateRedactor.Invoke("HasJsonArrayEnd", JsonArrayValue));
        }

        [TestMethod]
        public void HasJsonArrayEnd_WhenJsonArrayValueIsNotNullAndDoesNotEndWithJsonArrayEnd_ShouldReturnFalse()
        {
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
            var privateRedactor = new PrivateObject(redactor);
            var JsonArrayValue = "This does not have Json Array end.";

            Assert.IsFalse((bool)privateRedactor.Invoke("HasJsonArrayStart", JsonArrayValue));
        }

        [TestMethod]
        public void HasJsonArrayEnd_WhenJsonArrayValueIsNull_ShouldReturnFalse()
        {
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
            var privateRedactor = new PrivateObject(redactor);
            var JsonArrayValue = null as string;

            Assert.IsFalse((bool)privateRedactor.Invoke("HasJsonArrayEnd", JsonArrayValue));
        }
        #endregion

        #region BuildJsonRedactor
        [TestMethod]
        public void BuildJsonRedactor_WhenRedactByIsNameOrPatternOrNameAndPattern_ShouldReturnJsonRedactorWithParent()
        {
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
            var privateRedactor = new PrivateObject(redactor);

            var returnValue = privateRedactor.Invoke("BuildJsonRedactor") as IRedactor;

            Assert.AreEqual(typeof(JsonRedactor), returnValue.GetType());
            Assert.AreEqual(redactor, returnValue.ParentRedactor as Redactor);

        }

        [TestMethod]
        public void BuildJsonRedactor_WhenRedactByIsNotNameOrPatternOrNameAndPattern_ShouldReturnNull()
        {
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.None));
            var privateRedactor = new PrivateObject(redactor);

            var returnValue = privateRedactor.Invoke("BuildJsonRedactor") as IRedactor;

            Assert.AreEqual(null, returnValue);
        }
        #endregion

        #region IsPiiName
        [TestMethod]
        public void IsPiiName_WhenNothingIsNullAndNameContainsAnyRedactNamesValue_ShouldReturnTrue()
        {
            using (ShimsContext.Create())
            {
                var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateRedactor = new PrivateObject(redactor);
                var name = "MatchingValue";
                ShimRedactor.AllInstances.RedactNamesGet = (x) => { return new List<string> { name }; };

                Assert.IsTrue((bool)privateRedactor.Invoke("IsPiiName", name));
            }
        }

        [TestMethod]
        public void IsPiiName_WhenNothingIsNullAndNameDoesNotContainAnyRedactNamesValue_ShouldReturnFalse()
        {
            using (ShimsContext.Create())
            {
                var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateRedactor = new PrivateObject(redactor);
                var name = "MatchingValue";
                ShimRedactor.AllInstances.RedactNamesGet = (x) => { return new List<string> { "NotMatchingValue" }; };

                Assert.IsFalse((bool)privateRedactor.Invoke("IsPiiName", name));
            }
        }

        [TestMethod]
        public void IsPiiName_WhenRedactNamesIsNull_ShouldReturnFalse()
        {
            using (ShimsContext.Create())
            {
                var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateRedactor = new PrivateObject(redactor);
                var name = "MatchingValue";
                ShimRedactor.AllInstances.RedactNamesGet = (x) => { return null; };

                Assert.IsFalse((bool)privateRedactor.Invoke("IsPiiName", name));
            }
        }

        [TestMethod]
        public void IsPiiName_WhenRedactNameMembersAreNull_ShouldReturnFalse()
        {
            using (ShimsContext.Create())
            {
                var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateRedactor = new PrivateObject(redactor);
                var name = "MatchingValue";
                ShimRedactor.AllInstances.RedactNamesGet = (x) => { return new List<string> { null }; };

                Assert.IsFalse((bool)privateRedactor.Invoke("IsPiiName", name));
            }
        }

        [TestMethod]
        public void IsPiiName_WhenNameIsNull_ShouldReturnFalse()
        {
            using (ShimsContext.Create())
            {
                var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateRedactor = new PrivateObject(redactor);
                ShimRedactor.AllInstances.RedactNamesGet = (x) => { return new List<string> { "IsPiiName" }; };

                Assert.IsFalse((bool)privateRedactor.Invoke("IsPiiName", (string)null));
            }
        }
        #endregion

        #region GetRedactedValue
        [TestMethod]
        public void GetRedactedValue_WhenValueToRedactIsNullOrEmpty_ShouldReturnValueToRedact()
        {
            var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
            var privateRedactor = new PrivateObject(redactor);
            var valueToRedact1 = null as string;
            var valueToRedact2 = string.Empty;

            var returnValue1 = privateRedactor.Invoke("GetRedactedValue", valueToRedact1, false) as string;
            var returnValue2 = privateRedactor.Invoke("GetRedactedValue", valueToRedact2, false) as string;

            Assert.AreEqual(valueToRedact1, returnValue1);
            Assert.AreEqual(valueToRedact2, returnValue2);
        }

        [TestMethod]
        public void GetRedactedValue_WhenValueToRedactIsNotNullOrEmptyAndRedactByNameIsTrue_ShouldReturnNameRedactValue()
        {
            using (ShimsContext.Create())
            {
                var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateRedactor = new PrivateObject(redactor);
                var valueToRedact = "SomeValue";
                var nameRedacted = "NameRedacted";
                ShimRedactor.AllInstances.NameRedactValueGet = (x) => { return nameRedacted; };

                var returnValue = privateRedactor.Invoke("GetRedactedValue", valueToRedact, true) as string;

                Assert.AreEqual(nameRedacted, returnValue);
            }
        }

        [TestMethod]
        public void GetRedactedValue_WhenValueToRedactIsNotNullOrEmptyAndRedactByNameIsFalseAndTryRedactReturnsTrue_ShouldReturnRedactedValue()
        {
            using (ShimsContext.Create())
            {
                var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateRedactor = new PrivateObject(redactor);
                var valueToRedact = "John";
                var redactedValue = "IWasRedacted";
                ShimRedactor.AllInstances.TryRedactStringStringOut = (Redactor x, string y, out string z) => 
                { 
                    z = redactedValue; 
                    return true; 
                };

                var returnValue = privateRedactor.Invoke("GetRedactedValue", valueToRedact, false) as string;

                Assert.AreEqual(redactedValue, returnValue);
            }
        }

        [TestMethod]
        public void GetRedactedValue_WhenValueToRedactIsNotNullOrEmptyAndRedactByNameIsFalseAndTryRedactReturnsFalseAndRedactPatternIsTrue_ShouldReturnGetPatternRedactedValue()
        {
            using (ShimsContext.Create())
            {
                var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateRedactor = new PrivateObject(redactor);
                var valueToRedact = "John";
                var redactedValue = "IWasRedacted";
                ShimRedactor.AllInstances.TryRedactStringStringOut = (Redactor x, string y, out string z) =>
                {
                    z = null;
                    return false;
                };
                ShimRedactor.AllInstances.RedactPatternGet = (x) => { return true; };
                ShimRedactor.AllInstances.GetPatternRedactedValueString = (x, y) => { return redactedValue; };

                var returnValue = privateRedactor.Invoke("GetRedactedValue", valueToRedact, false) as string;

                Assert.AreEqual(redactedValue, returnValue);
            }
        }

        [TestMethod]
        public void GetRedactedValue_WhenValueToRedactIsNullOrEmptyAndRedactByNameIsFalseAndTryRedactReturnsFalseAndRedactPatternIsFalse_ShouldReturnValueToRedact()
        {
            using (ShimsContext.Create())
            {
                var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateRedactor = new PrivateObject(redactor);
                var valueToRedact = "John";
                ShimRedactor.AllInstances.TryRedactStringStringOut = (Redactor x, string y, out string z) =>
                {
                    z = null;
                    return false;
                };
                ShimRedactor.AllInstances.RedactPatternGet = (x) => { return false; };

                var returnValue = privateRedactor.Invoke("GetRedactedValue", valueToRedact, false) as string;

                Assert.AreEqual(valueToRedact, returnValue);
            }
        }
        #endregion

        #region TryRedact
        [TestMethod]
        public void TryRedact_WhenRedactorTypeIsXmlAndValueToRedactHasJsonEdgesAndTryRedactJsonReturnsTrue_ShouldOutRedactedValueAndReturnTrue()
        {
            using (ShimsContext.Create())
            {
                var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateRedactor = new PrivateObject(redactor);
                var valueToRedact = "John";
                var redactedValue = "IWasRedacted";
                ShimRedactor.AllInstances.RedactorTypeGet = (x) => { return RedactorType.Xml; };
                ShimRedactor.AllInstances.HasJsonEdgesString = (x, y) => { return true; };
                ShimRedactor.AllInstances.TryRedactJsonStringStringOut = (Redactor x, string y, out string z) =>
                {
                    z = redactedValue;
                    return true;
                };
                var parameters = new object[] { valueToRedact, string.Empty };

                var returnValue = (bool)privateRedactor.Invoke("TryRedact", parameters);

                Assert.IsTrue(returnValue);
                Assert.AreEqual(redactedValue, parameters[1]);
            }
        }

        [TestMethod]
        public void TryRedact_WhenRedactorTypeIsXmlAndValueToRedactHasJsonEdgesAndTryRedactJsonReturnsFalse_ShouldOutNullAndReturnFalse()
        {
            using (ShimsContext.Create())
            {
                var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateRedactor = new PrivateObject(redactor);
                var valueToRedact = "John";
                var redactedValue = "IWasRedacted";
                ShimRedactor.AllInstances.RedactorTypeGet = (x) => { return RedactorType.Xml; };
                ShimRedactor.AllInstances.HasJsonEdgesString = (x, y) => { return true; };
                ShimRedactor.AllInstances.TryRedactJsonStringStringOut = (Redactor x, string y, out string z) =>
                {
                    z = redactedValue;
                    return false;
                };
                var parameters = new object[] { valueToRedact, string.Empty };

                var returnValue = (bool)privateRedactor.Invoke("TryRedact", parameters);

                Assert.IsFalse(returnValue);
                Assert.AreEqual(null, parameters[1]);
            }
        }

        [TestMethod]
        public void TryRedact_WhenRedactorTypeIsXmlAndValueToRedactDoesNotHaveJsonEdgesAndTryRedactJsonReturnsFalse_ShouldOutNullAndReturnFalse()
        {
            using (ShimsContext.Create())
            {
                var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateRedactor = new PrivateObject(redactor);
                var valueToRedact = "John";
                ShimRedactor.AllInstances.RedactorTypeGet = (x) => { return RedactorType.Xml; };
                ShimRedactor.AllInstances.HasJsonEdgesString = (x, y) => { return false; };
                var parameters = new object[] { valueToRedact, string.Empty };

                var returnValue = (bool)privateRedactor.Invoke("TryRedact", parameters);

                Assert.IsFalse(returnValue);
                Assert.AreEqual(null, parameters[1]);
            }
        }

        [TestMethod]
        public void TryRedact_WhenRedactorTypeIsJsonAndValueToRedactHasXmlEdgesAndTryRedactXmlReturnsTrue_ShouldOutRedactedValueAndReturnTrue()
        {
            using (ShimsContext.Create())
            {
                var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateRedactor = new PrivateObject(redactor);
                var valueToRedact = "John";
                var redactedValue = "IWasRedacted";
                ShimRedactor.AllInstances.RedactorTypeGet = (x) => { return RedactorType.Json; };
                ShimRedactor.AllInstances.HasXmlEdgesString = (x, y) => { return true; };
                ShimRedactor.AllInstances.TryRedactXmlStringStringOut = (Redactor x, string y, out string z) =>
                {
                    z = redactedValue;
                    return true;
                };
                var parameters = new object[] { valueToRedact, string.Empty };

                var returnValue = (bool)privateRedactor.Invoke("TryRedact", parameters);

                Assert.IsTrue(returnValue);
                Assert.AreEqual(redactedValue, parameters[1]);
            }
        }

        [TestMethod]
        public void TryRedact_WhenRedactorTypeIsJsonAndValueToRedactHasJsonEdgesAndTryRedactJsonReturnsFalse_ShouldOutNullAndReturnFalse()
        {
            using (ShimsContext.Create())
            {
                var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateRedactor = new PrivateObject(redactor);
                var valueToRedact = "John";
                var redactedValue = "IWasRedacted";
                ShimRedactor.AllInstances.RedactorTypeGet = (x) => { return RedactorType.Json; };
                ShimRedactor.AllInstances.HasJsonEdgesString = (x, y) => { return true; };
                ShimRedactor.AllInstances.TryRedactJsonStringStringOut = (Redactor x, string y, out string z) =>
                {
                    z = redactedValue;
                    return false;
                };
                var parameters = new object[] { valueToRedact, string.Empty };

                var returnValue = (bool)privateRedactor.Invoke("TryRedact", parameters);

                Assert.IsFalse(returnValue);
                Assert.AreEqual(null, parameters[1]);
            }
        }

        [TestMethod]
        public void TryRedact_WhenRedactorTypeIsJsonAndValueToRedactDoesNotHaveJsonEdgesAndTryRedactJsonReturnsFalse_ShouldOutNullAndReturnFalse()
        {
            using (ShimsContext.Create())
            {
                var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateRedactor = new PrivateObject(redactor);
                var valueToRedact = "John";
                ShimRedactor.AllInstances.RedactorTypeGet = (x) => { return RedactorType.Json; };
                ShimRedactor.AllInstances.HasJsonEdgesString = (x, y) => { return false; };
                var parameters = new object[] { valueToRedact, string.Empty };

                var returnValue = (bool)privateRedactor.Invoke("TryRedact", parameters);

                Assert.IsFalse(returnValue);
                Assert.AreEqual(null, parameters[1]);
            }
        }

        [TestMethod]
        public void TryRedact_WhenRedactorTypeIsNone_ShouldOutNullAndReturnFalse()
        {
            using (ShimsContext.Create())
            {
                var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateRedactor = new PrivateObject(redactor);
                var valueToRedact = "John";
                ShimRedactor.AllInstances.RedactorTypeGet = (x) => { return RedactorType.None; };
                var parameters = new object[] { valueToRedact, string.Empty };

                var returnValue = (bool)privateRedactor.Invoke("TryRedact", parameters);

                Assert.IsFalse(returnValue);
                Assert.AreEqual(null, parameters[1]);
            }
        }
        #endregion

        #region TryRedactXml
        [TestMethod]
        public void TryRedactXml_WhenXmlRedactorRedactReturnsRedactedValue_ShouldOutRedactedValueAndReturnTrue()
        {
            using (ShimsContext.Create())
            {
                var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateRedactor = new PrivateObject(redactor);
                var valueToRedact = "John";
                var redactedValue = "IWasRedacted";
                ShimXmlRedactor.AllInstances.RedactString = (x, y) => { return redactedValue; };
                var parameters = new object[] { valueToRedact, string.Empty };

                var returnValue = (bool)privateRedactor.Invoke("TryRedactXml", parameters);

                Assert.IsTrue(returnValue);
                Assert.AreEqual(redactedValue, parameters[1]);
            }
        }

        [TestMethod]
        public void TryRedactXml_WhenXmlRedactorRedactThrowsException_ShouldOutNullAndReturnFalse()
        {
            using (ShimsContext.Create())
            {
                var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateRedactor = new PrivateObject(redactor);
                var valueToRedact = "John";
                ShimXmlRedactor.AllInstances.RedactString = (x, y) => { throw new Exception(); };
                var parameters = new object[] { valueToRedact, string.Empty };

                var returnValue = (bool)privateRedactor.Invoke("TryRedactXml", parameters);

                Assert.IsFalse(returnValue);
                Assert.AreEqual(null, parameters[1]);
            }
        }
        #endregion

        #region TryRedactJson
        [TestMethod]
        public void TryRedactJson_WhenJsonRedactorRedactReturnsRedactedValue_ShouldOutRedactedValueAndReturnTrue()
        {
            using (ShimsContext.Create())
            {
                var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateRedactor = new PrivateObject(redactor);
                var valueToRedact = "John";
                var redactedValue = "IWasRedacted";
                ShimJsonRedactor.AllInstances.RedactString = (x, y) => { return redactedValue; };
                var parameters = new object[] { valueToRedact, string.Empty };

                var returnValue = (bool)privateRedactor.Invoke("TryRedactJson", parameters);

                Assert.IsTrue(returnValue);
                Assert.AreEqual(redactedValue, parameters[1]);
            }
        }

        [TestMethod]
        public void TryRedactJson_WhenJsonRedactorRedactThrowsException_ShouldOutNullAndReturnFalse()
        {
            using (ShimsContext.Create())
            {
                var redactor = new Redactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateRedactor = new PrivateObject(redactor);
                var valueToRedact = "John";
                ShimJsonRedactor.AllInstances.RedactString = (x, y) => { throw new Exception(); };
                var parameters = new object[] { valueToRedact, string.Empty };

                var returnValue = (bool)privateRedactor.Invoke("TryRedactJson", parameters);

                Assert.IsFalse(returnValue);
                Assert.AreEqual(null, parameters[1]);
            }
        }
        #endregion

        #region GetPatternRedactedValue
        [TestMethod]
        public void GetPatternRedactedValue_WhenValueLengthIsGreaterThanOrEqualToMinimumLength_ShouldReturnValueWithPatternsReplaced()
        {
            using (ShimsContext.Create())
            {
                var config = RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern);
                var redactor = new Redactor(config);
                var privateRedactor = new PrivateObject(redactor);
                var valueToRedact = "user@domain.com | NonSensitiveString";
                ShimRedactor.AllInstances.RedactPatternsGet = (x) => { return config.RedactPatterns.Where(y => y.Name == "EMAIL-ADDRESS"); };
                var expectedReturnValue = "*REDACTED-EMAIL-ADDRESS* | NonSensitiveString";

                var returnValue = privateRedactor.Invoke("GetPatternRedactedValue", valueToRedact) as string;

                Assert.AreEqual(expectedReturnValue, returnValue);
            }
        }

        [TestMethod]
        public void GetPatternRedactedValue_WhenValueLengthIsLessThanMinimumLength_ShouldReturnValue()
        {
            using (ShimsContext.Create())
            {
                var config = RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern);
                var redactor = new Redactor(config);
                var privateRedactor = new PrivateObject(redactor);
                var valueToRedact = "user";
                ShimRedactor.AllInstances.RedactPatternsGet = (x) => { return config.RedactPatterns.Where(y => y.Name == "EMAIL-ADDRESS"); };
                var expectedReturnValue = valueToRedact;

                var returnValue = privateRedactor.Invoke("GetPatternRedactedValue", valueToRedact) as string;

                Assert.AreEqual(expectedReturnValue, returnValue);
            }
        }

        [TestMethod]
        public void GetPatternRedactedValue_WhenRedactPatternsIsEmpty_ShouldReturnValue()
        {
            using (ShimsContext.Create())
            {
                var config = RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern);
                var redactor = new Redactor(config);
                var privateRedactor = new PrivateObject(redactor);
                var valueToRedact = "user@domain.com | NonSensitiveString";
                ShimRedactor.AllInstances.RedactPatternsGet = (x) => { return config.RedactPatterns.Where(y => 1 == 2); };
                var expectedReturnValue = valueToRedact;

                var returnValue = privateRedactor.Invoke("GetPatternRedactedValue", valueToRedact) as string;

                Assert.AreEqual(expectedReturnValue, returnValue);
            }
        }

        [TestMethod]
        public void GetPatternRedactedValue_WhenRedactPatternsIsNull_ShouldReturnValue()
        {
            using (ShimsContext.Create())
            {
                var config = RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern);
                var redactor = new Redactor(config);
                var privateRedactor = new PrivateObject(redactor);
                var valueToRedact = "user@domain.com | NonSensitiveString";
                ShimRedactor.AllInstances.RedactPatternsGet = (x) => { return null; };
                var expectedReturnValue = valueToRedact;

                var returnValue = privateRedactor.Invoke("GetPatternRedactedValue", valueToRedact) as string;

                Assert.AreEqual(expectedReturnValue, returnValue);
            }
        }
        #endregion
        #endregion
    }
}
