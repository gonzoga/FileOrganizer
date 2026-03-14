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

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void GetSubFolder_InvalidFilePath_ReturnsEmptyString(string? filePath)
        {
            var result = _metadataEngine.GetSubFolder(filePath);
            Assert.Equal("", result);
        }

        [Fact]
        public void GetSubFolder_NonExistentFile_ReturnsEmptyString()
        {
            string path = Path.Combine(_tempDirectory, "doesnotexist.jpg");
            var result = _metadataEngine.GetSubFolder(path);
            Assert.Equal("", result);
        }

        [Fact]
        public void GetSubFolder_UnhandledExtension_ReturnsEmptyString()
        {
            string path = Path.Combine(_tempDirectory, "test.txt");
            File.WriteAllText(path, "dummy text content");
            var result = _metadataEngine.GetSubFolder(path);
            Assert.Equal("", result);
        }

        [Fact]
        public void GetSubFolder_EmptyAudioFile_ReturnsUnknownArtist()
        {
            // TagLib# will throw CorruptFileException for empty/invalid mp3
            string path = Path.Combine(_tempDirectory, "empty.mp3");
            File.WriteAllBytes(path, Array.Empty<byte>());
            var result = _metadataEngine.GetSubFolder(path);
            Assert.Equal("Unknown Artist", result);
        }

        [Fact]
        public void GetSubFolder_InvalidImageFile_ReturnsUnsortedPhotos()
        {
            // MetadataExtractor will fail to read dimensions/EXIF
            string path = Path.Combine(_tempDirectory, "invalid.jpg");
            File.WriteAllText(path, "not a real image");
            var result = _metadataEngine.GetSubFolder(path);
            Assert.Equal("Unsorted Photos", result);
        }

        [Fact]
        public void GetSubFolder_SmallPngImage_ReturnsIconsAndGraphics()
        {
            string path = Path.Combine(_tempDirectory, "icon.png");

            // Minimal 1x1 valid PNG byte array
            byte[] png1x1 = new byte[] {
                0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a,
                0x00, 0x00, 0x00, 0x0d, 0x49, 0x48, 0x44, 0x52,
                0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, // width 1, height 1
                0x08, 0x06, 0x00, 0x00, 0x00, 0x1f, 0x15, 0xc4, 0x89,
                0x00, 0x00, 0x00, 0x0a, 0x49, 0x44, 0x41, 0x54,
                0x78, 0x9c, 0x63, 0x00, 0x01, 0x00, 0x00, 0x05, 0x00, 0x01,
                0x0d, 0x0a, 0x2d, 0xb4, 0x00, 0x00, 0x00, 0x00,
                0x49, 0x45, 0x4e, 0x44, 0xae, 0x42, 0x60, 0x82
            };

            File.WriteAllBytes(path, png1x1);
            var result = _metadataEngine.GetSubFolder(path);
            Assert.Equal("Icons & Graphics", result);
        }

        [Fact]
        public void GetSubFolder_LargePngImage_ReturnsUnsortedPhotos()
        {
            string path = Path.Combine(_tempDirectory, "large.png");

            // Minimal 512x512 valid PNG byte array
            byte[] png512x512 = new byte[] {
                0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a,
                0x00, 0x00, 0x00, 0x0d, 0x49, 0x48, 0x44, 0x52,
                0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x02, 0x00, // width 512, height 512
                0x08, 0x02, 0x00, 0x00, 0x00, 0x90, 0x2b, 0xba, 0x25,
                0x00, 0x00, 0x00, 0x0b, 0x49, 0x44, 0x41, 0x54,
                0x08, 0xd7, 0x63, 0xf8, 0xff, 0xff, 0x3f, 0x00, 0x05, 0xfe, 0x02, 0xfe, 0xa3, 0xe1, 0x24, 0x76,
                0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4e, 0x44, 0xae, 0x42, 0x60, 0x82
            };

            File.WriteAllBytes(path, png512x512);
            var result = _metadataEngine.GetSubFolder(path);
            Assert.Equal("Unsorted Photos", result);
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
