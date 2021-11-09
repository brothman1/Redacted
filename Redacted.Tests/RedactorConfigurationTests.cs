using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Newtonsoft.Json;
using Redacted.Configuration;

namespace Redacted.Tests
{
    [TestClass]
    public class RedactorConfigurationTests
    {
        [TestMethod]
        public void DataContract_WhenInputIsName_ShouldDeserializeWithCorrectMembersInitialized()
        {
            var serializedValue = RedactedResource.Get("RedactorConfiguration_Name");

            var config = JsonConvert.DeserializeObject<RedactorConfiguration>(serializedValue);

            Assert.AreNotEqual(null, config);
            Assert.AreEqual(RedactBy.Name, config.RedactBy);
            Assert.AreNotEqual(RedactedResource.DefaultNameRedactValue, config.NameRedactValue);
            Assert.AreNotEqual(null, config.RedactNames);
            Assert.AreEqual(null, config.RedactPatterns);
            Assert.AreEqual(null, config.RedactPatterns);
            Assert.AreEqual(null, config.RedactPatterns?[0]?.Name ?? null);
            Assert.AreEqual(null, config.RedactPatterns?[0]?.Pattern ?? null);
            Assert.AreEqual(null, config.RedactPatterns?[0]?.MinimumLength ?? null);
        }

        [TestMethod]
        public void DataContract_WhenInputIsPattern_ShouldDeserializeWithCorrectMembersInitialized()
        {
            var serializedValue = RedactedResource.Get("RedactorConfiguration_Pattern"); ;

            var config = JsonConvert.DeserializeObject<RedactorConfiguration>(serializedValue);

            Assert.AreNotEqual(null, config);
            Assert.AreEqual(RedactBy.Pattern, config.RedactBy);
            Assert.AreEqual(RedactedResource.DefaultNameRedactValue, config.NameRedactValue);
            Assert.AreEqual(null, config.RedactNames);
            Assert.AreNotEqual(null, config.RedactPatterns);
            Assert.AreNotEqual(null, config.RedactPatterns[0].Name);
            Assert.AreNotEqual(null, config.RedactPatterns[0].Pattern);
            Assert.AreNotEqual(null, config.RedactPatterns[0].MinimumLength);
        }

        [TestMethod]
        public void DataContract_WhenInputIsNameAndPattern_ShouldDeserializeWithCorrectMembersInitialized()
        {
            var serializedValue = RedactedResource.Get("RedactorConfiguration_NameAndPattern"); ;

            var config = JsonConvert.DeserializeObject<RedactorConfiguration>(serializedValue);

            Assert.AreNotEqual(null, config);
            Assert.AreEqual(RedactBy.NameAndPattern, config.RedactBy);
            Assert.AreEqual(RedactedResource.DefaultNameRedactValue, config.NameRedactValue);
            Assert.AreNotEqual(null, config.RedactNames);
            Assert.AreNotEqual(null, config.RedactPatterns);
            Assert.AreNotEqual(null, config.RedactPatterns[0].Name);
            Assert.AreNotEqual(null, config.RedactPatterns[0].Pattern);
            Assert.AreNotEqual(null, config.RedactPatterns[0].MinimumLength);
        }
    }
}
