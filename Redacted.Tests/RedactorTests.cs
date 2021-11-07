using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Newtonsoft.Json;
using Redacted;
using Redacted.Configuration;

namespace Redacted.Tests
{
    [TestClass]
    public class RedactorTests
    {
        private const string ResourcesDirectory = @"..\..\Resources\";



        [TestMethod]
        public void Blah()
        {
            var blah = GetResource("RedactorConfiguration_NameAndPattern");
            var config = JsonConvert.DeserializeObject<RedactorConfiguration>(blah);
        }

        private static string GetResource(string name, string extension = "json")
        {
            return File.ReadAllText($"{ResourcesDirectory}{name}.{extension}");
        }
    }
}
