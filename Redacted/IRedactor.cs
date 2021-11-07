using System.Collections.Generic;
using Redacted.Configuration;

namespace Redacted
{
    public interface IRedactor
    {
        int Id { get; }

        IEnumerable<string> RedactNames { get; }

        IEnumerable<RedactPattern> RedactPatterns { get; }

        string NameRedactValue { get; }

        RedactBy RedactBy { get; }

        bool RedactName { get; }

        bool RedactPattern { get; }

        IRedactor ParentRedactor { get; }

        IRedactor XmlRedactor { get; }

        IRedactor JsonRedactor { get; }

        string Redact(string valueToRedact);
    }
}
