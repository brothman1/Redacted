using System;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Redacted.Configuration
{
    [DataContract]
    public class RedactPattern
    {
        private const long DefaultMatchTimeoutTicks = 10000;
        private Regex _regex;

        /// <summary>
        /// Compiled regex representing <see cref="Pattern"/> with a match timeout of <see cref="MatchTimeoutTicks"/>.
        /// </summary>
        [IgnoreDataMember]
        public Regex Regex => _regex ?? BuildRegex();

        /// <summary>
        /// Name of the PiiPattern.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Pattern to match against.
        /// </summary>
        [DataMember]
        public string Pattern { get; set; }

        /// <summary>
        /// Minimum length required to match pattern.
        /// </summary>
        [DataMember]
        public int MinimumLength { get; set; }

        /// <summary>
        /// Number of 100 nanosecond ticks before regex timeout.
        /// </summary>
        [DataMember]
        public long MatchTimeoutTicks { get; set; } = DefaultMatchTimeoutTicks;

        /// <summary>
        /// Builds compiled regex representing <see cref="Pattern"/> with a match timeout of <see cref="MatchTimeoutTicks"/>.
        /// </summary>
        /// <returns><see cref="_regex"/> after assigning it.</returns>
        private Regex BuildRegex()
        {
            _regex = new Regex(Pattern, RegexOptions.Compiled, new TimeSpan(MatchTimeoutTicks));
            return _regex;
        }
    }
}