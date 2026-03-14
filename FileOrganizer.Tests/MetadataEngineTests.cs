using System;
using System.Reflection;
using FileOrganizer.Engines;
using Xunit;

namespace FileOrganizer.Tests
{
    public class MetadataEngineTests
    {
        private readonly MetadataEngine _engine = new MetadataEngine();

        private string CallSanitizeForPath(string input)
        {
            var method = typeof(MetadataEngine).GetMethod("SanitizeForPath", BindingFlags.NonPublic | BindingFlags.Instance);
            return (string)method.Invoke(_engine, new object[] { input });
        }

        [Theory]
        [InlineData("ValidName", "ValidName")]
        [InlineData("  TrimmedName  ", "TrimmedName")]
        [InlineData("Name/With/Slash", "NameWithSlash")]
        [InlineData("Name\\With\\Backslash", "NameWithBackslash")]
        [InlineData("..", "Unknown")]
        [InlineData("../etc/passwd", "etcpasswd")]
        [InlineData(".", "Unknown")]
        [InlineData("valid.name", "validname")]
        public void SanitizeForPath_RemovesInvalidAndPathTraversalChars(string input, string expected)
        {
            string actual = CallSanitizeForPath(input);
            Assert.Equal(expected, actual);
        }
    }
}
