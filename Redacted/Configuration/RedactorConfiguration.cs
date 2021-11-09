using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Redacted.Configuration
{
    [DataContract]
    public class RedactorConfiguration : IRedactorConfiguration
    {
        [IgnoreDataMember]
        private const string DefaultNameRedactValue = @"*REDACTED-NAME*";

        [DataMember]
        /// <summary>
        /// Value to indicate whether to redact by name, pattern, or both.
        /// </summary>
        public RedactBy RedactBy { get; set; }

        [DataMember]
        /// <summary>
        /// Value to replace name matches with.
        /// </summary>
        public string NameRedactValue { get; set; } = DefaultNameRedactValue;

        [DataMember]
        /// <summary>
        /// List of property names that hold the values needed to redact by pattern.
        /// </summary>
        public List<string> RedactNames { get; set; }

        [DataMember]
        /// <summary>
        /// List of <see cref="RedactPattern"/> that holds the values needed to redact by pattern.
        /// </summary>
        public List<RedactPattern> RedactPatterns { get; set; }
    }
}