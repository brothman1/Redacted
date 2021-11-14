namespace Redacted
{
    public enum RedactBy
    {
        // Remove me
        None,

        /// <summary>
        /// Redact by matching property names.
        /// </summary>
        Name,

        /// <summary>
        /// Redact by matching values to patterns.
        /// </summary>
        Pattern,

        /// <summary>
        /// Redact by matching property names and by matching values to patterns.
        /// </summary>
        NameAndPattern
    }
}
