using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redacted.Tests
{
    internal static class RedactedResource
    {
        private const string ResourcesDirectory = @"..\..\Resources\";
        
        internal static string Get(string name, string extension = "json")
        {
            return File.ReadAllText($"{ResourcesDirectory}{name}.{extension}");
        }
    }
}
