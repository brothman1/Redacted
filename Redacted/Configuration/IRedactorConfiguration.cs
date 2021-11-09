using System.Collections.Generic;

namespace Redacted.Configuration
{
    public interface IRedactorConfiguration
    {
        string NameRedactValue { get; set; }
        RedactBy RedactBy { get; set; }
        List<string> RedactNames { get; set; }
        List<RedactPattern> RedactPatterns { get; set; }
    }
}