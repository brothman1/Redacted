using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Redacted.Configuration;
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
        private static readonly Lazy<string> _lazyDefaultNameRedactValue = new Lazy<string>(() =>
        {
            var redactorConfiguration = new PrivateType(typeof(RedactorConfiguration));
            return redactorConfiguration.GetStaticFieldOrProperty("DefaultNameRedactValue") as string;
        });
        private static readonly Lazy<List<string>> _lazyRedactNames = new Lazy<List<string>>(() =>
        {
            return new List<string>
            {
                "Address",
                "Name",
                "Email",
                "PhoneNumber",
                "SocialSecurityNumber",
                "SSN",
                "TaxID",
                "DateOfBirth",
                "DOB"
            };
        });
        private static readonly Lazy<List<RedactPattern>> _lazyRedactPatterns = new Lazy<List<RedactPattern>>(() =>
        {
            return new List<RedactPattern>
            {
                new RedactPattern() { Name = "CC-VISA-MSC-DSC", Pattern = "[456](([^\\dA-Za-z]?)+\\d){15}", MinimumLength = 16 },
                new RedactPattern() { Name = "CC-AMEX", Pattern = "3(([^\\dA-Za-z]?)+\\d){14}", MinimumLength = 15 },
                new RedactPattern() { Name = "EMAIL-ADDRESS", Pattern = "@\"[^ @]+@+[^@]+\\.+[A-Za-z]+\"", MinimumLength = 5 },
                new RedactPattern() { Name = "NINE-DIGITS-PLUS", Pattern = "\\d(([^\\dA-Za-z]?)+\\d){8,}", MinimumLength = 9 },
                new RedactPattern() { Name = "IP-ADDRESS", Pattern = "\\d+(([ ]?)+\\.+([ ]?)+\\d+){3,}", MinimumLength = 7 },
                new RedactPattern() { Name = "DATE-SHORT-AMBIGUOUS", Pattern = "([1-2]\\d|3[0-1]|0?\\d)([\\\\/.,_-])([1-2]\\d|3[0-1]|0?\\d)\\2(?:\\d{4}|\\d{2})", MinimumLength = 6 },
                new RedactPattern() { Name = "DATE-SHORT-MONTH-DAY-YEAR", Pattern = "(?<!\\d)(?:(?:1[0-2]|0?\\d)(?=([\\\\/.,_-])(1[3-9]|2\\d|3[0-1])))\\1\\2\\1(?:\\d{4}|\\d{2})(?!\\d)", MinimumLength = 8 },
                new RedactPattern() { Name = "DATE-SHORT-DAY-MONTH-YEAR", Pattern = "(?<!\\d)(?:(?:1[3-9]|2\\d|3[0-1])(?=([\\\\/.,_-])(1[0-2]|0?\\d)))\\1\\2\\1(?:\\d{4}|\\d{2})(?!\\d)", MinimumLength = 8 },
                new RedactPattern() { Name = "DATE-SHORT-YEAR-MONTH-DAY", Pattern = "(?<!\\d)(?:(?:\\d{4}|\\d{2})(?=([\\\\/.,_-])(1[0-2]|0?\\d)))\\1\\2\\1(?:1[3-9]|2\\d|3[0-1])(?!\\d)", MinimumLength = 8 },
            };
        });
        private const string ResourcesDirectory = @"..\..\Resources\";
        
        internal static string DefaultNameRedactValue => _lazyDefaultNameRedactValue.Value;

        internal static List<string> RedactNames => _lazyRedactNames.Value;

        internal static List<RedactPattern> RedactPatterns => _lazyRedactPatterns.Value;

        internal static string Get(string name, string extension = "json")
        {
            return File.ReadAllText($"{ResourcesDirectory}{name}.{extension}");
        }

        #region GetMockedRedactorConfiguration
        internal static IRedactorConfiguration GetMockedRedactorConfiguration(RedactBy redactBy)
        {
            var config = new Mock<IRedactorConfiguration>();
            config.SetupGet(x => x.RedactBy).Returns(redactBy);
            config.SetupGet(x => x.NameRedactValue).Returns(DefaultNameRedactValue);

            if (redactBy == RedactBy.Name || redactBy == RedactBy.NameAndPattern)
            {
                SetupRedactNames(config);
            }

            if (redactBy == RedactBy.Pattern || redactBy == RedactBy.NameAndPattern)
            {
                SetupRedactPatterns(config);
            }

            return config.Object;
        }

        private static void SetupRedactNames(Mock<IRedactorConfiguration> config)
        {
            config.SetupGet(x => x.RedactNames).Returns(RedactNames);
        }

        private static void SetupRedactPatterns(Mock<IRedactorConfiguration> config)
        {
            config.SetupGet(x => x.RedactPatterns).Returns(RedactPatterns);
        }
        #endregion

        private static RedactPattern GetMockedPattern(string name, string pattern, int minimumLength)
        {
            var redactPattern = new Mock<RedactPattern>();
            redactPattern.SetupGet(x => x.Name).Returns(name);
            redactPattern.SetupGet(x => x.Pattern).Returns(pattern);
            redactPattern.SetupGet(x => x.MinimumLength).Returns(minimumLength);
            return redactPattern.Object;
        }
    }
}
