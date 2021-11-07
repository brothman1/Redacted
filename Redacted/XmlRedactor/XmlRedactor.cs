using System;
using System.IO;
using System.Xml.Linq;
using Redacted.Configuration;

namespace Redacted
{
    public class XmlRedactor : Redactor
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRedactor"/> class that will redact by matching property names or value patterns.
        /// </summary>
        /// <param name="config"><see cref="RedactorConfiguration"/> that houses the configurating used to redact.</param>
        /// <exception cref="ArgumentNullException"><paramref name="config"/> cannot be null.</exception>
        public XmlRedactor(RedactorConfiguration config) : base(config)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRedactor"/> class that will redact by matching property names or value patterns.
        /// </summary>
        /// <param name="config"><see cref="RedactorConfiguration"/> that houses the configurating used to redact.</param>
        /// <param name="parentRedactor"><see cref="Redactor"/> that created this.</param>
        /// <exception cref="ArgumentNullException"><paramref name="config"/> cannot be null.</exception>
        public XmlRedactor(RedactorConfiguration config, IRedactor parentRedactor) : base(config)
        {
            ParentRedactor = parentRedactor;
        }
        #endregion

        /// <summary>
        /// Redacts <paramref name="xmlToRedact"/> by either matching propery names, value patterns, or both depending on the value of <see cref="RedactBy"/>.
        /// </summary>
        /// <param name="xmlToRedact">The serialized XML string to redact.</param>
        /// <returns>The redacted version of <paramref name="xmlToRedact"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="xmlToRedact"/> must be in valid XML format.</exception>
        public override string Redact(string xmlToRedact)
        {
            var nodeToRedact = GetXml(xmlToRedact.Trim());
            RedactNode(nodeToRedact);
            return nodeToRedact.ToString();
        }


        /// <summary>
        /// Gets <see cref="XNode"/> based on <paramref name="xmlToRedact"/>.
        /// </summary>
        /// <param name="xmlToRedact">The serialized XML string.</param>
        /// <exception cref="ArgumentException"><paramref name="xmlToRedact"/> must be in valid XML format.</exception>
        private XNode GetXml(string xmlToRedact)
        {
            try
            {
                return XDocument.Load(new StringReader(xmlToRedact));
            }
            catch (Exception e)
            {
                var message = $"Unable to deserialize \"{nameof(xmlToRedact)}\". This is likely not valid XML, see inner exception for more details.";
                throw new ArgumentException(message, nameof(xmlToRedact), e);
            }
        }

        /// <summary>
        /// Calls child methods to process or redact members of <paramref name="nodeToRedact"/>.
        /// </summary>
        /// <remarks>Start point to redact any <see cref="XNode"/>.</remarks>
        /// <param name="nodeToRedact"><see cref="XNode"/> that is to be redacted.</param>
        private void RedactNode(XNode nodeToRedact)
        {
            ProcessChildAttributes(nodeToRedact);
            ProcessChildNodes(nodeToRedact);
            RedactProperty(nodeToRedact);
            RedactValue(nodeToRedact);
        }

        /// <summary>
        /// Loops through all <see cref="XAttribute"/> children to redact.
        /// </summary>
        /// <remarks>Determines <paramref name="parentNode"/> is <see cref="XElement"/> and has attributes before looping.</remarks>
        /// <param name="parentNode"><see cref="XNode"/> to redact <see cref="XAttribute"/> children from.</param>
        private void ProcessChildAttributes(XNode parentNode)
        {
            if (parentNode is XElement element && element.HasAttributes)
            {
                var attributeToRedact = element.FirstAttribute;
                while (attributeToRedact != null)
                {
                    RedactProperty(attributeToRedact);
                    attributeToRedact = attributeToRedact.NextAttribute;
                }
            }
        }

        /// <summary>
        /// Loops through all <see cref="XNode"/> children to redact.
        /// </summary>
        /// <remarks>Determines <paramref name="parentNode"/> is <see cref="XContainer"/> before looping.</remarks>
        /// <param name="parentNode"><see cref="XNode"/> to redact <see cref="XNode"/> children from.</param>
        private void ProcessChildNodes(XNode parentNode)
        {
            if (parentNode is XContainer container)
            {
                var nodeToRedact = container.FirstNode;
                while (nodeToRedact != null)
                {
                    RedactNode(nodeToRedact);
                    nodeToRedact = nodeToRedact.NextNode;
                }
            }
        }

        #region RedactProperty
        /// <summary>
        /// Redacts values from property objects.
        /// </summary>
        /// <remarks><para>Determines <paramref name="propertyObject"/> is either <see cref="XAttribute"/> or returns true from 
        /// <see cref="TryGetPropertyAndValue(XObject, out XElement, out XText)"/> before redacting.</para><para>"property 
        /// object" here is loosely defined as an object with a name and a single value.</para></remarks>
        /// <param name="propertyObject"><see cref="XObject"/> that is to be redacted.</param>
        private void RedactProperty(XObject propertyObject)
        {
            if (propertyObject is XAttribute attribute)
            {
                RedactValue(propertyObject, RedactName && IsPiiName(attribute.Name.LocalName));
            }
            else if (TryGetPropertyAndValue(propertyObject, out XElement element, out XText text))
            {
                RedactValue(text, RedactName && IsPiiName(element.Name.LocalName));
            }
        }

        /// <summary>
        /// Determines if <paramref name="propertyObject"/> is a <see cref="XElement"/> property object, outputs it converted 
        /// to <see cref="XElement"/>, and outputs its value.
        /// </summary>
        /// <remarks>"property object" here is loosely defined as an object with a name and a single value.</remarks>
        /// <param name="propertyObject"><see cref="XObject"/> that we attempt to retrieve <paramref name="property"/> and
        /// <paramref name="value"/> from.</param>
        /// <param name="property"><see cref="XElement"/> that outputs if <paramref name="propertyObject"/> is a property 
        /// object.</param>
        /// <param name="value"><see cref="XText"/> that outputs if <paramref name="propertyObject"/> is a property object.
        /// </param>
        /// <returns>True if <paramref name="propertyObject"/> is <see cref="XElement"/>, has elements, and its first node is
        /// <see cref="XText"/></returns>
        private bool TryGetPropertyAndValue(XObject propertyObject, out XElement property, out XText value)
        {
            if (propertyObject is XElement element && !element.HasElements && element.FirstNode is XText text)
            {
                property = element;
                value = text;
                return true;
            }
            else
            {
                property = null;
                value = null;
                return false;
            }
        }
        #endregion

        /// <summary>
        /// Redacts values from value objects.
        /// </summary>
        /// <remarks><para>Determines <paramref name="valueObject"/> is either <see cref="XComment"/>, <see cref="XAttribute"/> or
        /// <see cref="XText"/> before redacting.</para><para>"value object" here is loosely defined as an object that directly 
        /// contains a single value.</para></remarks>
        /// <param name="valueObject"><see cref="XObject"/> that is to be redacted.</param>
        /// <param name="redactByName"><see cref="bool"/> that indicates whether the value should be redacted based on name.</param>
        private void RedactValue(XObject valueObject, bool redactByName = false)
        {
            if (valueObject is XComment comment)
            {
                comment.Value = GetRedactedValue(comment.Value, redactByName);
            }
            else if (valueObject is XAttribute attribute)
            {
                attribute.Value = GetRedactedValue(attribute.Value, redactByName);
            }
            else if (valueObject is XText text)
            {
                text.Value = GetRedactedValue(text.Value, redactByName);
            }
        }


    }
}
