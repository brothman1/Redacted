using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Xml.Linq;
using Redacted.Fakes;

namespace Redacted.Tests
{
    [TestClass]
    public class XmlRedactorTests
    {
        #region Constructor
        [TestMethod]
        public void Constuctor_WhenConfigIsNotNull_ShouldInstantiateNewRedactor()
        {
            var nameConfig = RedactedResource.GetMockedRedactorConfiguration(RedactBy.Name);
            var patternConfig = RedactedResource.GetMockedRedactorConfiguration(RedactBy.Pattern);
            var nameAndPatternConfig = RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern);

            var nameXmlRedactor = new XmlRedactor(nameConfig);
            var patternXmlRedactor = new XmlRedactor(patternConfig);
            var nameAndPatternXmlRedactor = new XmlRedactor(nameAndPatternConfig);

            Assert.AreNotEqual(null, nameXmlRedactor);
            Assert.AreNotEqual(null, patternXmlRedactor);
            Assert.AreNotEqual(null, nameAndPatternXmlRedactor);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WhenConfigIsNull_ShouldThrowArgumentNullException()
        {
            _ = new XmlRedactor(null);
        }
        #endregion

        #region Methods
        #region  Redact
        [TestMethod]
        public void Redact_WhenXmlToRedactIsValidXml_ShouldReturnRedactedXmlValue()
        {
            var xmlToRedact = RedactedResource.Get("Redactor", "xml");
            var expectedValue = RedactedResource.Get("Redactor_Redacted", "xml");
            var xmlRedactor = new XmlRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));

            var returnValue = xmlRedactor.Redact(xmlToRedact);

            Assert.AreEqual(expectedValue.RemoveWhitespace(), returnValue.RemoveWhitespace());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Redact_WhenXmlToRedactIsNotValidXml_ShouldThrowArgumentException()
        {
            var xmlToRedact = "<Not XML>>";
            var xmlRedactor = new XmlRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));

            _ = xmlRedactor.Redact(xmlToRedact);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Redact_WhenXmlToRedactIsNull_ShouldThrowArgumentException()
        {
            var xmlToRedact = null as string;
            var xmlRedactor = new XmlRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));

            _ = xmlRedactor.Redact(xmlToRedact);
        }
        #endregion

        #region GetXml
        [TestMethod]
        public void GetXml_WhenXmlToRedactIsValidXml_ShouldReturnDeserializedXml()
        {
            var xmlToRedact = RedactedResource.Get("Redactor", "xml");
            var xmlRedactor = new XmlRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
            var privateXmlRedactor = new PrivateObject(xmlRedactor);

            var returnValue = privateXmlRedactor.Invoke("GetXml", xmlToRedact) as XNode;

            Assert.AreNotEqual(null, returnValue);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetXml_WhenXmlToRedactIsNotValidXml_ShouldThrowArgumentException()
        {
            var xmlToRedact = "<Not XML>>";
            var xmlRedactor = new XmlRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
            var privateXmlRedactor = new PrivateObject(xmlRedactor);

            _ = privateXmlRedactor.Invoke("GetXml", xmlToRedact);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetXml_WhenXmlToRedactIsNull_ShouldThrowArgumentException()
        {
            var xmlToRedact = null as string;
            var xmlRedactor = new XmlRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
            var privateXmlRedactor = new PrivateObject(xmlRedactor);

            _ = privateXmlRedactor.Invoke("GetXml", xmlToRedact);
        }
        #endregion

        [TestMethod]
        public void RedactNode_ShouldCallProcessChildAttributesAndProcessChildNodesAndRedactPropertyAndRedactValue()
        {
            using (ShimsContext.Create())
            {
                var xmlRedactor = new XmlRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateXmlRedactor = new PrivateObject(xmlRedactor);
                var calledProcessChildAttributes = false;
                var calledProcessChildNodes = false;
                var calledRedactProperty = false;
                var calledRedactValue = false;
                ShimXmlRedactor.AllInstances.ProcessChildAttributesXNode = (x, y) => { calledProcessChildAttributes = true; };
                ShimXmlRedactor.AllInstances.ProcessChildNodesXNodeBoolean = (x, y, z) => { calledProcessChildNodes = true; };
                ShimXmlRedactor.AllInstances.RedactPropertyXObject = (x, y) => { calledRedactProperty = true; };
                ShimXmlRedactor.AllInstances.RedactValueXObjectBoolean = (x, y, z) => { calledRedactValue = true; };

                privateXmlRedactor.Invoke("RedactNode", null as XNode, false);

                Assert.IsTrue(calledProcessChildAttributes);
                Assert.IsTrue(calledProcessChildNodes);
                Assert.IsTrue(calledRedactProperty);
                Assert.IsTrue(calledRedactValue);
            }
        }

        #region ProcessChildAttributes
        [TestMethod]
        public void ProcessChildAttributes_WhenParentNodeIsXElementAndHasAttributes_ShouldCallRedactPropertyOncePerAttribute()
        {
            using (ShimsContext.Create())
            {
                var xmlRedactor = new XmlRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateXmlRedactor = new PrivateObject(xmlRedactor);
                var parentNode = new XElement(XName.Get("SomeName"));
                parentNode.SetAttributeValue(XName.Get("Attribute1"), "SomeValue1");
                parentNode.SetAttributeValue(XName.Get("Attribute2"), "SomeValue2");
                parentNode.SetAttributeValue(XName.Get("Attribute3"), "SomeValue3");
                var redactPropertyCalls = 0;
                ShimXmlRedactor.AllInstances.RedactPropertyXObject = (x, y) => { redactPropertyCalls++; };


                privateXmlRedactor.Invoke("ProcessChildAttributes", parentNode);

                Assert.AreEqual(3, redactPropertyCalls);
            }
        }

        [TestMethod]
        public void ProcessChildAttributes_WhenParentNodeIsXElementAndDoesNotHaveAttributes_ShouldNotCallRedactProperty()
        {
            using (ShimsContext.Create())
            {
                var xmlRedactor = new XmlRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateXmlRedactor = new PrivateObject(xmlRedactor);
                var parentNode = new XElement(XName.Get("SomeName"));
                var calledRedactProperty = false;
                ShimXmlRedactor.AllInstances.RedactPropertyXObject = (x, y) => { calledRedactProperty = true; };

                privateXmlRedactor.Invoke("ProcessChildAttributes", parentNode);

                Assert.IsFalse(calledRedactProperty);
            }
        }

        [TestMethod]
        public void ProcessChildAttributes_WhenParentNodeIsNotXElement_ShouldNotCallRedactProperty()
        {
            using (ShimsContext.Create())
            {
                var xmlRedactor = new XmlRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateXmlRedactor = new PrivateObject(xmlRedactor);
                var calledRedactProperty = false;
                ShimXmlRedactor.AllInstances.RedactPropertyXObject = (x, y) => { calledRedactProperty = true; };

                privateXmlRedactor.Invoke("ProcessChildAttributes", null as XNode);

                Assert.IsFalse(calledRedactProperty);
            }
        }
        #endregion

        #region ProcessChildNodes
        [TestMethod]
        public void ProcessChildNodes_WhenParentNodeIsXContainerAndRedactByNameIsTrue_ShouldCallRedactNodeOncePerChildAndNotCallHasPiiParent()
        {
            using (ShimsContext.Create())
            {
                var xmlRedactor = new XmlRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateXmlRedactor = new PrivateObject(xmlRedactor);
                var parentNode = new XElement(XName.Get("SomeName"));
                parentNode.SetElementValue(XName.Get("Element1"), "SomeValue1");
                parentNode.SetElementValue(XName.Get("Element2"), "SomeValue2");
                parentNode.SetElementValue(XName.Get("Element3"), "SomeValue3");
                var redactNodeCalls = 0;
                ShimXmlRedactor.AllInstances.RedactNodeXNodeBoolean = (x, y, z) => { redactNodeCalls++; };
                var hasPiiParentCalls = 0;
                ShimXmlRedactor.AllInstances.HasPiiParentXNode = (x, y) =>
                {
                    hasPiiParentCalls++;
                    return true;
                };

                privateXmlRedactor.Invoke("ProcessChildNodes", parentNode, true);

                Assert.AreEqual(3, redactNodeCalls);
                Assert.AreEqual(0, hasPiiParentCalls);
            }
        }

        [TestMethod]
        public void ProcessChildNodes_WhenParentNodeIsXContainerAndRedactByNameIsFalse_ShouldCallRedactNodeAndHasPiiParentOncePerChild()
        {
            using (ShimsContext.Create())
            {
                var xmlRedactor = new XmlRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateXmlRedactor = new PrivateObject(xmlRedactor);
                var parentNode = new XElement(XName.Get("SomeName"));
                parentNode.SetElementValue(XName.Get("Element1"), "SomeValue1");
                parentNode.SetElementValue(XName.Get("Element2"), "SomeValue2");
                parentNode.SetElementValue(XName.Get("Element3"), "SomeValue3");
                var redactNodeCalls = 0;
                ShimXmlRedactor.AllInstances.RedactNodeXNodeBoolean = (x, y, z) => { redactNodeCalls++; };
                var hasPiiParentCalls = 0;
                ShimXmlRedactor.AllInstances.HasPiiParentXNode = (x, y) =>
                {
                    hasPiiParentCalls++;
                    return true;
                };

                privateXmlRedactor.Invoke("ProcessChildNodes", parentNode, false);

                Assert.AreEqual(3, redactNodeCalls);
                Assert.AreEqual(3, hasPiiParentCalls);
            }
        }

        [TestMethod]
        public void ProcessChildNodes_WhenParentNodeIsNotXContainer_ShouldNotCallRedactNodeOrHasPiiParent()
        {
            using (ShimsContext.Create())
            {
                var xmlRedactor = new XmlRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateXmlRedactor = new PrivateObject(xmlRedactor);
                var redactNodeCalled = false;
                ShimXmlRedactor.AllInstances.RedactNodeXNodeBoolean = (x, y, z) => { redactNodeCalled = true; };
                var hasPiiParentCalled = false;
                ShimXmlRedactor.AllInstances.HasPiiParentXNode = (x, y) =>
                {
                    hasPiiParentCalled = true;
                    return true;
                };

                privateXmlRedactor.Invoke("ProcessChildNodes", null, false);

                Assert.IsFalse(redactNodeCalled);
                Assert.IsFalse(hasPiiParentCalled);
            }
        }
        #endregion

        #region HasPiiParent
        [TestMethod]
        public void HasPiiParent_WhenNodeToRedactsParentIsNotNullAndIsXElementAndRedactNameIsTrueAndIsPiiNameReturnsTrue_ShouldReturnTrue()
        {
            using (ShimsContext.Create())
            {
                var xmlRedactor = new XmlRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateXmlRedactor = new PrivateObject(xmlRedactor);
                var parentNode = new XElement(XName.Get("SomeName"));
                parentNode.SetElementValue(XName.Get("Element1"), "SomeValue1");
                var nodeToRedact = parentNode.FirstNode;
                ShimRedactor.AllInstances.RedactNameGet = (x) => true;
                ShimRedactor.AllInstances.IsPiiNameString = (x, y) => true;

                Assert.IsTrue((bool)privateXmlRedactor.Invoke("HasPiiParent", nodeToRedact));
            }
        }

        [TestMethod]
        public void HasPiiParent_WhenNodeToRedactsParentIsNotNullAndIsXElementAndRedactNameIsTrueAndIsPiiNameReturnsFalse_ShouldReturnFalse()
        {
            using (ShimsContext.Create())
            {
                var xmlRedactor = new XmlRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateXmlRedactor = new PrivateObject(xmlRedactor);
                var parentNode = new XElement(XName.Get("SomeName"));
                parentNode.SetElementValue(XName.Get("Element1"), "SomeValue1");
                var nodeToRedact = parentNode.FirstNode;
                ShimRedactor.AllInstances.RedactNameGet = (x) => true;
                ShimRedactor.AllInstances.IsPiiNameString = (x, y) => false;

                Assert.IsFalse((bool)privateXmlRedactor.Invoke("HasPiiParent", nodeToRedact));
            }
        }

        [TestMethod]
        public void HasPiiParent_WhenNodeToRedactsParentIsNotNullAndIsXElementAndRedactNameIsFalseAndIsPiiNameReturnsFalse_ShouldReturnFalse()
        {
            using (ShimsContext.Create())
            {
                var xmlRedactor = new XmlRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateXmlRedactor = new PrivateObject(xmlRedactor);
                var parentNode = new XElement(XName.Get("SomeName"));
                parentNode.SetElementValue(XName.Get("Element1"), "SomeValue1");
                var nodeToRedact = parentNode.FirstNode;
                ShimRedactor.AllInstances.RedactNameGet = (x) => false;
                ShimRedactor.AllInstances.IsPiiNameString = (x, y) => false;

                Assert.IsFalse((bool)privateXmlRedactor.Invoke("HasPiiParent", nodeToRedact));
            }
        }

        [TestMethod]
        public void HasPiiParent_WhenNodeToRedactsParentIsNotNullAndIsNotXElement_ShouldReturnFalse()
        {
            using (ShimsContext.Create())
            {
                var xmlRedactor = new XmlRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateXmlRedactor = new PrivateObject(xmlRedactor);
                var parentNode = new XElement(XName.Get("SomeName"));
                parentNode.SetElementValue(XName.Get("Element1"), "SomeValue1");
                var nodeToRedact = ((XElement)parentNode.FirstNode).FirstNode;

                Assert.IsFalse((bool)privateXmlRedactor.Invoke("HasPiiParent", nodeToRedact));
            }
        }

        [TestMethod]
        public void HasPiiParent_WhenNodeToRedactOrNodeToRedactsParentIsNull_ShouldReturnFalse()
        {
            using (ShimsContext.Create())
            {
                var xmlRedactor = new XmlRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateXmlRedactor = new PrivateObject(xmlRedactor);
                var nodeToRedact = new XElement(XName.Get("SomeName"));

                Assert.IsFalse((bool)privateXmlRedactor.Invoke("HasPiiParent", nodeToRedact));
                Assert.IsFalse((bool)privateXmlRedactor.Invoke("HasPiiParent", null as XNode));
            }
        }
        #endregion

        #region RedactProperty
        [TestMethod]
        public void RedactProperty_WhenPropertyIsXAttributeAndRedactNameIsTrue_ShouldCallRedactValueAndIsPiiName()
        {
            using (ShimsContext.Create())
            {
                var xmlRedactor = new XmlRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateXmlRedactor = new PrivateObject(xmlRedactor);
                var propertyObject = new XAttribute(XName.Get("Attribute1"), "SomeValue1");
                var redactValueCalled = false;
                ShimXmlRedactor.AllInstances.RedactValueXObjectBoolean = (x, y, z) => redactValueCalled = true;
                ShimRedactor.AllInstances.RedactNameGet = (x) => true;
                var isPiiNameCalled = false;
                ShimRedactor.AllInstances.IsPiiNameString = (x, y) =>
                {
                    isPiiNameCalled = true;
                    return true;
                };

                privateXmlRedactor.Invoke("RedactProperty", propertyObject);

                Assert.IsTrue(redactValueCalled);
                Assert.IsTrue(isPiiNameCalled);
            }
        }

        [TestMethod]
        public void RedactProperty_WhenPropertyIsXAttributeAndRedactNameIsFalse_ShouldCallRedactValueAndNotCallIsPiiName()
        {
            using (ShimsContext.Create())
            {
                var xmlRedactor = new XmlRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateXmlRedactor = new PrivateObject(xmlRedactor);
                var propertyObject = new XAttribute(XName.Get("Attribute1"), "SomeValue1");
                var redactValueCalled = false;
                ShimXmlRedactor.AllInstances.RedactValueXObjectBoolean = (x, y, z) => redactValueCalled = true;
                ShimRedactor.AllInstances.RedactNameGet = (x) => false;
                var isPiiNameCalled = false;
                ShimRedactor.AllInstances.IsPiiNameString = (x, y) =>
                {
                    isPiiNameCalled = true;
                    return true;
                };

                privateXmlRedactor.Invoke("RedactProperty", propertyObject);

                Assert.IsTrue(redactValueCalled);
                Assert.IsFalse(isPiiNameCalled);
            }
        }

        [TestMethod]
        public void RedactProperty_WhenPropertyIsNotXAttributeAndTryGetPropertyAndValueReturnsTrueAndRedactNameIsTrue_ShouldCallRedactValueAndIsPiiName()
        {
            using (ShimsContext.Create())
            {
                var xmlRedactor = new XmlRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateXmlRedactor = new PrivateObject(xmlRedactor);
                var propertyObject = new XElement(XName.Get("Attribute1"), "SomeValue1");
                ShimXmlRedactor.AllInstances.TryGetPropertyAndValueXObjectXElementOutXTextOut = (XmlRedactor w, XObject x, out XElement y, out XText z) =>
                {
                    y = null;
                    z = null;
                    return true;
                };
                var redactValueCalled = false;
                ShimXmlRedactor.AllInstances.RedactValueXObjectBoolean = (x, y, z) => redactValueCalled = true;
                ShimRedactor.AllInstances.RedactNameGet = (x) => true;
                var isPiiNameCalled = false;
                ShimRedactor.AllInstances.IsPiiNameString = (x, y) =>
                {
                    isPiiNameCalled = true;
                    return true;
                };

                privateXmlRedactor.Invoke("RedactProperty", propertyObject);

                Assert.IsTrue(redactValueCalled);
                Assert.IsTrue(isPiiNameCalled);
            }
        }

        [TestMethod]
        public void RedactProperty_WhenPropertyIsNotXAttributeAndTryGetPropertyAndValueReturnsTrueAndRedactNameIsFalse_ShouldCallRedactValueAndNotCallIsPiiName()
        {
            using (ShimsContext.Create())
            {
                var xmlRedactor = new XmlRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateXmlRedactor = new PrivateObject(xmlRedactor);
                var propertyObject = new XElement(XName.Get("Attribute1"), "SomeValue1");
                ShimXmlRedactor.AllInstances.TryGetPropertyAndValueXObjectXElementOutXTextOut = (XmlRedactor w, XObject x, out XElement y, out XText z) =>
                {
                    y = null;
                    z = null;
                    return true;
                };
                var redactValueCalled = false;
                ShimXmlRedactor.AllInstances.RedactValueXObjectBoolean = (x, y, z) => redactValueCalled = true;
                ShimRedactor.AllInstances.RedactNameGet = (x) => false;
                var isPiiNameCalled = false;
                ShimRedactor.AllInstances.IsPiiNameString = (x, y) =>
                {
                    isPiiNameCalled = true;
                    return true;
                };

                privateXmlRedactor.Invoke("RedactProperty", propertyObject);

                Assert.IsTrue(redactValueCalled);
                Assert.IsFalse(isPiiNameCalled);
            }
        }

        [TestMethod]
        public void RedactProperty_WhenPropertyIsNotXAttributeAndTryGetPropertyAndValueReturnsFalse_ShouldNotCallRedactValueOrIsPiiName()
        {
            using (ShimsContext.Create())
            {
                var xmlRedactor = new XmlRedactor(RedactedResource.GetMockedRedactorConfiguration(RedactBy.NameAndPattern));
                var privateXmlRedactor = new PrivateObject(xmlRedactor);
                ShimXmlRedactor.AllInstances.TryGetPropertyAndValueXObjectXElementOutXTextOut = (XmlRedactor w, XObject x, out XElement y, out XText z) =>
                {
                    y = null;
                    z = null;
                    return false;
                };
                var redactValueCalled = false;
                ShimXmlRedactor.AllInstances.RedactValueXObjectBoolean = (x, y, z) => redactValueCalled = true;
                var isPiiNameCalled = false;
                ShimRedactor.AllInstances.IsPiiNameString = (x, y) =>
                {
                    isPiiNameCalled = true;
                    return true;
                };

                privateXmlRedactor.Invoke("RedactProperty", null as XObject);

                Assert.IsFalse(redactValueCalled);
                Assert.IsFalse(isPiiNameCalled);
            }
        }
        #endregion

        #region TryGetPropertyAndValue
        [TestMethod]
        public void TryGetPropertyAndValue_WhenPropertyObjectIsXElementAndHasNoElementsAndFirstNodeIsXText_ShouldOutPropertyObjectAsXElementAndFirstNodeAsXTextAndReturnTrue()
        {

        }

        [TestMethod]
        public void TryGetPropertyAndValue_WhenPropertyObjectIsXElementAndHasNoElemntsAndFirstNodeIsNotXText_ShouldOutNullPropertyAndValueAndReturnFalse()
        {

        }

        [TestMethod]
        public void TryGetPropertyAndValue_WhenPropertyObjectIsXElementAndHasElements_ShouldOutNullPropertyAndValueAndReturnFalse()
        {

        }

        [TestMethod]
        public void TryGetPropertyAndValue_WhenPropertyObjectIsNotXElement_ShouldOutNullPropertyAndValueAndReturnFalse()
        {

        }

        [TestMethod]
        public void TryGetPropertyAndValue_WhenPropertyObjectIsNull_ShouldOutNullPropertyAndValueAndReturnFalse()
        {

        }
        #endregion

        #region RedactValue
        [TestMethod]
        public void RedactValue_WhenValueObjectIsXComment_ShouldAssignXCommentValueAndCallGetRedactedValue()
        {

        }

        [TestMethod]
        public void RedactValue_WhenValueObjectIsXAttribute_ShouldAssignAttributeValueAndCallGetRedactedValue()
        {

        }

        [TestMethod]
        public void RedactedValue_WhenValueObjectIsXText_ShouldAssignXTextValueAndCallGetRedactedValue()
        {

        }

        [TestMethod]
        public void RedactedValue_WhenValueObjectIsNotXCommentOrXAttributeOrXText_ShouldNotCallGetRedactedValue()
        {

        }
        #endregion
        #endregion
    }
}
