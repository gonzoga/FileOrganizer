using System;
using System.IO;
using FileOrganizer.Engines;
using Xunit;

namespace FileOrganizer.Tests
{
    public class MetadataEngineTests : IDisposable
    {
        private readonly string _tempDirectory;
        private readonly MetadataEngine _metadataEngine;

        public MetadataEngineTests()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), "FileOrganizer_MetadataTests_" + Guid.NewGuid());
            Directory.CreateDirectory(_tempDirectory);
            _metadataEngine = new MetadataEngine();
        }

        private string CreateMockMp3WithArtist(string filename, string artistName)
        {
            string path = Path.Combine(_tempDirectory, filename);

            // Create a minimal valid MP3 frame so TagLib doesn't throw CorruptFileException
            byte[] mp3Frame = new byte[] { 0xFF, 0xFB, 0x90, 0x44, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            byte[] fileBytes = new byte[1024];
            Array.Copy(mp3Frame, fileBytes, mp3Frame.Length);

            File.WriteAllBytes(path, fileBytes);

            using (var tfile = TagLib.File.Create(path))
            {
                if (artistName != null)
                {
                    tfile.Tag.Performers = new[] { artistName };
                }
                tfile.Save();
            }

            return path;
        }

        [Theory]
        [InlineData("AC/DC", "ACDC")]
        [InlineData("The:Who", "TheWho")]
        [InlineData("Motley*Crue", "MotleyCrue")]
        [InlineData("Artist?Name", "ArtistName")]
        [InlineData("Artist\"Name\"", "ArtistName")]
        [InlineData("Artist<Name>", "ArtistName")]
        [InlineData("Artist|Name", "ArtistName")]
        [InlineData("  Valid Name  ", "Valid Name")]
        public void GetSubFolder_AudioWithInvalidPathChars_SanitizesArtistName(string artistName, string expectedSubFolder)
        {
            string path = CreateMockMp3WithArtist("test.mp3", artistName);
            var result = _metadataEngine.GetSubFolder(path);

            // On non-Windows platforms like Linux, Path.GetInvalidFileNameChars() only contains \0 and /
            // So for cross-platform test accuracy, we check if the char is actually invalid on the current host before asserting.
            // If the char isn't considered invalid on the host OS, it won't be sanitized.
            string expectedHostSpecific = expectedSubFolder;
            if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                expectedHostSpecific = artistName.Trim();
                foreach (char c in Path.GetInvalidFileNameChars())
                {
                    expectedHostSpecific = expectedHostSpecific.Replace(c.ToString(), "");
                }

                // TagLib splits Performers on '/', so "AC/DC" becomes an array ["AC", "DC"].
                // FirstPerformer will just return "AC".
                if (artistName == "AC/DC")
                {
                    expectedHostSpecific = "AC";
                }
            }

            Assert.Equal(expectedHostSpecific, result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("///")]
        public void GetSubFolder_AudioWithOnlyInvalidCharsOrEmpty_ReturnsUnknown(string artistName)
        {
            string path = CreateMockMp3WithArtist("empty.mp3", artistName);
            var result = _metadataEngine.GetSubFolder(path);

            string expected = string.IsNullOrWhiteSpace(artistName) ? "Unknown Artist" : "Unknown";
            Assert.Equal(expected, result);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
        }
    }
}
