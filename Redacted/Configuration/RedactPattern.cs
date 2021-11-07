using System.Runtime.Serialization;

namespace Redacted.Configuration
{
    [DataContract]
    public class RedactPattern
    {
        [DataMember]
        /// <summary>
        /// Name of the PiiPattern.
        /// </summary>
        public string Name { get; set; }

        [DataMember]
        /// <summary>
        /// Pattern to match against.
        /// </summary>
        public string Pattern { get; set; }

        [DataMember]
        /// <summary>
        /// Minimum length required to match pattern.
        /// </summary>
        public int MinimumLength { get; set; }
    }
}