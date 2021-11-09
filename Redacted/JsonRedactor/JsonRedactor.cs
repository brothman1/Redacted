using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Redacted.Configuration;

namespace Redacted
{
    public class JsonRedactor : Redactor
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonRedactor"/> class that will redact by matching property names or value patterns.
        /// </summary>
        /// <param name="config"><see cref="IRedactorConfiguration"/> that houses the configurating used to redact.</param>
        /// <exception cref="ArgumentNullException"><paramref name="config"/> cannot be null.</exception>
        public JsonRedactor(IRedactorConfiguration config) : base(config)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonRedactor"/> class that will redact by matching property names or value patterns.
        /// </summary>
        /// <param name="config"><see cref="IRedactorConfiguration"/> that houses the configurating used to redact.</param>
        /// <param name="parentRedactor"><see cref="Redactor"/> that created this.</param>
        /// <exception cref="ArgumentNullException"><paramref name="config"/> cannot be null.</exception>
        public JsonRedactor(IRedactorConfiguration config, IRedactor parentRedactor) : base(config)
        {
            ParentRedactor = parentRedactor;
        }
        #endregion

        /// <summary>
        /// Redacts <paramref name="jsonToRedact"/> by either matching propery names, value patterns, or both depending on the value of <see cref="RedactBy"/>.
        /// </summary>
        /// <param name="jsonToRedact">The serialized JSON string to redact.</param>
        /// <returns>The redacted version of <paramref name="jsonToRedact"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="jsonToRedact"/> must be in valid JSON format.</exception>
        public override string Redact(string jsonToRedact)
        {
            var tokenToRedact = GetJson(jsonToRedact.Trim());
            RedactToken(tokenToRedact);
            return JsonConvert.SerializeObject(tokenToRedact, Formatting.Indented);
        }

        /// <summary>
        /// Gets <see cref="JToken"/> based on <paramref name="jsonToRedact"/>.
        /// </summary>
        /// <param name="jsonToRedact">The serialized JSON string.</param>
        /// <exception cref="ArgumentException"><paramref name="jsonToRedact"/> must be in valid JSON format.</exception>
        private JToken GetJson(string jsonToRedact)
        {
            try
            {
                return (JToken)JsonConvert.DeserializeObject(jsonToRedact);
            }
            catch (Exception e)
            {
                var message = $"Unable to deserialize \"{nameof(jsonToRedact)}\". This is likely not valid JSON, see inner exception for more details.";
                throw new ArgumentException(message, nameof(jsonToRedact), e);
            }
        }

        private void RedactToken(JToken tokenToRedact, bool redactByName = false)
        {
            ProcessChildTokens(tokenToRedact, redactByName);
            RedactProperty(tokenToRedact);
            RedactValue(tokenToRedact, redactByName);
        }

        private void ProcessChildTokens(JToken parentToken, bool redactByName = false)
        {
            if (parentToken is JObject || parentToken is JArray)
            {
                var tokenToRedact = parentToken.First;
                while (tokenToRedact != null)
                {
                    RedactToken(tokenToRedact, redactByName || HasPiiParent(parentToken));
                    tokenToRedact = tokenToRedact.Next;
                }
            }
        }

        private bool HasPiiParent(JToken tokenToRedact)
        {
            if (tokenToRedact is JArray && tokenToRedact.Parent is JProperty parentProperty)
            {
                return RedactName && IsPiiName(parentProperty.Name);
            }
            else
            {
                return false;
            }
        }

        #region RedactProperty
        /// <summary>
        /// Redacts values from <see cref="JToken"/> objects.
        /// </summary>
        /// <remarks>Determines <paramref name="tokenToRedact"/>'s <see cref="JToken.Value"/> is <see cref="JToken"/> before redacting.</remarks>
        /// <param name="tokenToRedact"><see cref="JProperty"/> that is to be redacted.</param>
        private void RedactProperty(JToken tokenToRedact)
        {
            if (TryGetPropertyAndValue(tokenToRedact, out JProperty propertyToRedact, out JToken valueToRedact))
            {
                RedactValue(valueToRedact, RedactName && IsPiiName(propertyToRedact.Name));
            }
            else if (propertyToRedact != null && valueToRedact != null)
            {
                RedactToken(valueToRedact);
            }
        }


        #endregion

        private bool TryGetPropertyAndValue(JToken tokenToRedact, out JProperty propertyToRedact, out JToken valueToRedact)
        {
            if (TryGetProperty(tokenToRedact, out JProperty property) && property.Value is JValue value)
            {
                propertyToRedact = property;
                valueToRedact = value;
                return true;
            }
            else if (property != null)
            {
                propertyToRedact = property;
                valueToRedact = property.Value;
                return false;
            }
            else
            {
                propertyToRedact = null;
                valueToRedact = null;
                return false;
            }
        }

        private bool TryGetProperty(JToken tokenToRedact, out JProperty propertyToRedact)
        {
            if (tokenToRedact is JProperty property)
            {
                propertyToRedact = property;
                return true;
            }
            propertyToRedact = null;
            return false;
        }

        /// <summary>
        /// Redacts values from <see cref="JToken"/> objects.
        /// </summary>
        /// <param name="tokenToRedact"><see cref="JToken"/> that is to be redacted.</param>
        /// <param name="redactByName"><see cref="bool"/> that indicates whether the value should be redacted based on name.</param>
        private void RedactValue(JToken tokenToRedact, bool redactByName = false)
        {
            if (tokenToRedact is JValue valueToRedact && valueToRedact.Value is string value)
            {
                valueToRedact.Value = GetRedactedValue(value, redactByName);
            }
        }
    }
}
