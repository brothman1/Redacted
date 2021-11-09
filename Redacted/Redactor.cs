using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Redacted.Configuration;

namespace Redacted
{
    public class Redactor : IRedactor
    {
        private static readonly List<IRedactor> _redactorCollection = new List<IRedactor>();
        private const string XmlStart = "<";
        private const string XmlEnd = ">";
        private const string JsonObjectStart = "{";
        private const string JsonObjectEnd = "}";
        private const string JsonArrayStart = "[";
        private const string JsonArrayEnd = "]";
        private IRedactorConfiguration _config;
        private IRedactor _xmlRedactor = null;
        private IRedactor _jsonRedactor = null;

        #region Properties
        /// <summary>
        /// All Redactors
        /// </summary>
        public static IEnumerable<IRedactor> RedactorCollection => _redactorCollection;

        /// <summary>
        /// Ordinal representation of when this was instantiated.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// The names of properties whose matches to redact when <see cref="RedactName"/> is true.
        /// </summary>
        public IEnumerable<string> RedactNames => _config.RedactNames;

        /// <summary>
        /// The value that replaces property values redacted based on name matching.
        /// </summary>
        public string NameRedactValue => _config.NameRedactValue;

        /// <summary>
        /// The patterns whose value matches to redact when <see cref="RedactPattern"/> is true.
        /// </summary>
        public IEnumerable<RedactPattern> RedactPatterns => _config.RedactPatterns;

        /// <summary>
        /// The method by which redaction should take place, either <see cref="RedactBy.NameAndPattern"/>, <see cref="RedactBy.Name"/>, or 
        /// <see cref="RedactBy.Pattern"/>.
        /// </summary>
        public RedactBy RedactBy { get => _config.RedactBy; }

        /// <summary>
        /// True when <see cref="RedactBy"/> is <see cref="RedactBy.NameAndPattern"/> or <see cref="RedactBy.Name"/>.
        /// </summary>
        public bool RedactName => RedactBy == RedactBy.NameAndPattern || RedactBy == RedactBy.Name;

        /// <summary>
        /// True when <see cref="RedactBy"/> is <see cref="RedactBy.NameAndPattern"/> or <see cref="RedactBy.Pattern"/>.
        /// </summary>
        public bool RedactPattern => RedactBy == RedactBy.NameAndPattern || RedactBy == RedactBy.Pattern;

        /// <summary>
        /// Parent Redactor that created this redactor.
        /// </summary>
        public IRedactor ParentRedactor { get; protected set; }

        /// <summary>
        /// <see cref="Redacted.XmlRedactor"/> object to redact XML.
        /// </summary>
        public IRedactor XmlRedactor
        {
            get => _xmlRedactor ?? ParentRedactor?.XmlRedactor ?? BuildXmlRedactor();
            private set => _xmlRedactor = value;
        }

        /// <summary>
        /// <see cref="Redacted.JsonRedactor"/> object to redact JSON.
        /// </summary>
        public IRedactor JsonRedactor
        {
            get => _jsonRedactor ?? ParentRedactor?.JsonRedactor ?? BuildJsonRedactor();
            private set => _xmlRedactor = value;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="Redactor"/> class that will redact by matching property names or value patterns.
        /// </summary>
        /// <param name="config"><see cref="IRedactorConfiguration"/> that houses the configurating used to redact.</param>
        /// <exception cref="ArgumentNullException"><paramref name="config"/> cannot be null.</exception>
        public Redactor(IRedactorConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config), $"\"{nameof(config)}\" cannot be null.");
        }
        #endregion

        /// <summary>
        /// Redacts <paramref name="valueToRedact"/> by either matching propery names, value patterns, or both depending on the value of <see cref="RedactBy"/>.
        /// </summary>
        /// <param name="valueToRedact">The serialized XML or JSON string to redact.</param>
        /// <returns>The redacted version of <paramref name="valueToRedact"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="valueToRedact"/> must be in valid XML or JSON format.</exception>
        public virtual string Redact(string valueToRedact)
        {
            valueToRedact = valueToRedact?.Trim();
            if (HasXmlEdges(valueToRedact))
            {
                return XmlRedactor.Redact(valueToRedact);
            }
            else if (HasJsonEdges(valueToRedact))
            {
                return JsonRedactor.Redact(valueToRedact);
            }
            else
            {
                throw new ArgumentException($"\"{nameof(valueToRedact)}\" must be in either valid XML or valid JSON format to be redacted.", nameof(valueToRedact));
            }
        }

        #region Xml
        #region HasXmlEdges
        /// <summary>
        /// Determines if <paramref name="xmlValue"/> has valid edge characters for XML.
        /// </summary>
        /// <param name="xmlValue">The serialized XML string.</param>
        /// <returns>True if <paramref name="xmlValue"/> starts with <see cref="XmlStart"/> and ends with <see cref="XmlEnd"/>.</returns>
        private bool HasXmlEdges(string xmlValue) => HasXmlStart(xmlValue) && HasXmlEnd(xmlValue);

        /// <summary>
        /// Determines if <paramref name="xmlValue"/> has valid start character for XML.
        /// </summary>
        /// <param name="xmlValue">The serialized XML string.</param>
        /// <returns>True if <paramref name="xmlValue"/> starts with <see cref="XmlStart"/>.</returns>
        private bool HasXmlStart(string xmlValue) => xmlValue?.StartsWith(XmlStart) ?? false;

        /// <summary>
        /// Determines if <paramref name="xmlValue"/> has valid end character for XML.
        /// </summary>
        /// <param name="xmlValue">The serialized XML string.</param>
        /// <returns>True if <paramref name="xmlValue"/> ends with <see cref="XmlEnd"/>.</returns>
        private bool HasXmlEnd(string xmlValue) => xmlValue?.EndsWith(XmlEnd) ?? false;
        #endregion

        /// <summary>
        /// Builds new instance of <see cref="Redacted.XmlRedactor"/> by passing value types by reference.
        /// </summary>
        /// <returns><see cref="IRedactor"/></returns>
        /// <exception cref="InvalidOperationException"><see cref="RedactBy"/> must be either <see cref="RedactBy.NameAndPattern"/>, <see cref="RedactBy.Name"/>, or 
        /// <see cref="RedactBy.Pattern"/>.</exception>
        private IRedactor BuildXmlRedactor()
        {
            switch (RedactBy)
            {
                case RedactBy.NameAndPattern:
                    _xmlRedactor = new XmlRedactor(_config, this);
                    return _xmlRedactor;
                case RedactBy.Name:
                    _xmlRedactor = new XmlRedactor(_config, this);
                    return _xmlRedactor;
                case RedactBy.Pattern:
                    _xmlRedactor = new XmlRedactor(_config, this);
                    return _xmlRedactor;
                default:
                    var message = $"Can only build \"{nameof(Redacted.XmlRedactor)}\" when \"{nameof(Redactor)}.{nameof(RedactBy)}\" is " +
                                    $"\"{RedactBy.NameAndPattern}\", \"{RedactBy.Name}\", or \"{RedactBy.Pattern}\".";
                    throw new InvalidOperationException(message);
            }
        }
        #endregion

        #region Json
        #region HasJsonEdges
        /// <summary>
        /// Determines if <paramref name="jsonValue"/> has valid edge characters for JSON.
        /// </summary>
        /// <param name="jsonValue">The serialized JSON string.</param>
        /// <returns>True if <paramref name="jsonValue"/> starts with <see cref="JsonObjectStart"/> and ends with <see cref="JsonObjectEnd"/> or
        /// if <paramref name="jsonValue"/> starts with <see cref="JsonArrayStart"/> and ends with <see cref="JsonArrayEnd"/>.</returns>
        protected bool HasJsonEdges(string jsonValue) => HasJsonObjectEdges(jsonValue) || HasJsonArrayEdges(jsonValue);

        #region HasJsonObjectEdges
        /// <summary>
        /// Determines if <paramref name="jsonValue"/> has valid edge characters for a JSON object.
        /// </summary>
        /// <param name="jsonValue">The serialized JSON string.</param>
        /// <returns>True if <paramref name="jsonValue"/> starts with <see cref="JsonObjectStart"/> and ends with <see cref="JsonObjectEnd"/>.</returns>
        private bool HasJsonObjectEdges(string jsonValue) => HasJsonObjectStart(jsonValue) && HasJsonObjectEnd(jsonValue);

        /// <summary>
        /// Determines if <paramref name="jsonValue"/> has valid start character for a JSON object.
        /// </summary>
        /// <param name="jsonValue">The serialized JSON string.</param>
        /// <returns>True if <paramref name="jsonValue"/> is not null and starts with <see cref="JsonObjectStart"/>.</returns>
        private bool HasJsonObjectStart(string jsonValue) => jsonValue?.StartsWith(JsonObjectStart) ?? false;

        /// <summary>
        /// Determines if <paramref name="jsonValue"/> has valid end character for a JSON object.
        /// </summary>
        /// <param name="jsonValue">The serialized JSON string.</param>
        /// <returns>True if <paramref name="jsonValue"/> is not null and ends with <see cref="JsonObjectEnd"/>.</returns>
        private bool HasJsonObjectEnd(string jsonValue) => jsonValue?.EndsWith(JsonObjectEnd) ?? false;
        #endregion

        #region HasJsonArrayEdges
        /// <summary>
        /// Determines if <paramref name="jsonValue"/> has valid edge characters for a JSON array.
        /// </summary>
        /// <param name="jsonValue">The serialized JSON string.</param>
        /// <returns>True if <paramref name="jsonValue"/> starts with <see cref="JsonArrayStart"/> and ends with <see cref="JsonArrayEnd"/>.</returns>
        private bool HasJsonArrayEdges(string jsonValue) => HasJsonArrayStart(jsonValue) && HasJsonArrayEnd(jsonValue);

        /// <summary>
        /// Determines if <paramref name="jsonValue"/> has valid start character for a JSON array.
        /// </summary>
        /// <param name="jsonValue">The serialized JSON string.</param>
        /// <returns>True if <paramref name="jsonValue"/> is not null and starts with <see cref="JsonArrayStart"/>.</returns>
        private bool HasJsonArrayStart(string jsonValue) => jsonValue?.StartsWith(JsonArrayStart) ?? false;

        /// <summary>
        /// Determines if <paramref name="jsonValue"/> has valid end character for a JSON array.
        /// </summary>
        /// <param name="jsonValue">The serialized JSON string.</param>
        /// <returns>True if <paramref name="jsonValue"/> is not null and ends with <see cref="JsonArrayEnd"/>.</returns>
        private bool HasJsonArrayEnd(string jsonValue) => jsonValue?.EndsWith(JsonArrayEnd) ?? false;
        #endregion
        #endregion

        /// <summary>
        /// Builds new instance of <see cref="Redacted.JsonRedactor"/> by passing value types by reference.
        /// </summary>
        /// <returns><see cref="IRedactor"/></returns>
        /// <exception cref="InvalidOperationException"><see cref="RedactBy"/> must be either <see cref="RedactBy.NameAndPattern"/>, <see cref="RedactBy.Name"/>, or 
        /// <see cref="RedactBy.Pattern"/>.</exception>
        private IRedactor BuildJsonRedactor()
        {
            switch (RedactBy)
            {
                case RedactBy.NameAndPattern:
                    _jsonRedactor = new JsonRedactor(_config, this);
                    return _jsonRedactor;
                case RedactBy.Name:
                    _jsonRedactor = new JsonRedactor(_config, this);
                    return _jsonRedactor;
                case RedactBy.Pattern:
                    _jsonRedactor = new JsonRedactor(_config, this);
                    return _jsonRedactor;
                default:
                    var message = $"Can only build \"{nameof(Redacted.JsonRedactor)}\" when \"{nameof(Redactor)}.{nameof(RedactBy)}\" is " +
                                    $"\"{RedactBy.NameAndPattern}\", \"{RedactBy.Name}\", or \"{RedactBy.Pattern}\".";
                    throw new InvalidOperationException(message);
            }
        }
        #endregion

        /// <summary>
        /// Determines if <paramref name="name"/> matches <see cref="RedactNames"/>.
        /// </summary>
        /// <param name="name"><see cref="string"/> that is matched against <see cref="RedactNames"/>.</param>
        /// <returns>True if <paramref name="name"/> matches <see cref="RedactNames"/></returns>
        protected bool IsPiiName(string name) => RedactNames?.Any(x => name?.Trim().ToLower().Contains(x) ?? false) ?? false;

        /// <summary>
        /// Determines new value for objects based on <paramref name="valueToRedact"/>.
        /// </summary>
        /// <remarks>Determines <paramref name="valueToRedact"/> is not null or empty, and <paramref name="redactByName"/> or 
        /// <see cref="RedactPattern"/> are true before redacting.</remarks>
        /// <param name="valueToRedact"><see cref="string"/> that is to be redacted.</param>
        /// <param name="redactByName"><see cref="bool"/> that indicates whether the value should be redacted based on name.</param>
        /// <returns>Redacted version of <paramref name="valueToRedact"/>.</returns>
        protected string GetRedactedValue(string valueToRedact, bool redactByName)
        {
            if (string.IsNullOrEmpty(valueToRedact))
            {
                return valueToRedact;
            }
            else if (redactByName)
            {
                return NameRedactValue;
            }
            else if (TryRedact(valueToRedact, out string redactedValue))
            {
                return redactedValue;
            }
            else if (RedactPattern)
            {
                return GetPatternRedactedValue(valueToRedact);
            }
            else
            {
                return valueToRedact;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="valueToRedact"></param>
        /// <param name="redactedValue"></param>
        /// <returns></returns>
        private bool TryRedact(string valueToRedact, out string redactedValue)
        {
            if (this is XmlRedactor && HasJsonEdges(valueToRedact) && TryRedactJson(valueToRedact, out redactedValue))
            {

                return true;
            }
            else if (this is JsonRedactor && HasXmlEdges(valueToRedact) && TryRedactXml(valueToRedact, out redactedValue))
            {
                return true;
            }
            else
            {
                redactedValue = null;
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="valueToRedact"></param>
        /// <param name="redactedValue"></param>
        /// <returns></returns>
        private bool TryRedactJson(string valueToRedact, out string redactedValue)
        {
            try
            {
                redactedValue = JsonRedactor.Redact(valueToRedact);
                return true;
            }
            catch
            {
                redactedValue = null;
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="valueToRedact"></param>
        /// <param name="redactedValue"></param>
        /// <returns></returns>
        private bool TryRedactXml(string valueToRedact, out string redactedValue)
        {
            try
            {
                redactedValue = XmlRedactor.Redact(valueToRedact);
                return true;
            }
            catch
            {
                redactedValue = null;
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string GetPatternRedactedValue(string value)
        {
            if (RedactPattern)
            {
                //regex here
            }
            return value;
        }
    }
}
