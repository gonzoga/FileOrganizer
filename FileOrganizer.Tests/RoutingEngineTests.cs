using FileOrganizer.Engines;
using Xunit;

namespace FileOrganizer.Tests
{
    public class RoutingEngineTests
    {
        private readonly RoutingEngine _routingEngine;

        public RoutingEngineTests()
        {
            _routingEngine = new RoutingEngine();
        }

        [Theory]
        [InlineData("test.jpg", "Pictures")]
        [InlineData("image.jpeg", "Pictures")]
        [InlineData("pic.png", "Pictures")]
        [InlineData("anim.gif", "Pictures")]
        [InlineData("bitmap.bmp", "Pictures")]
        [InlineData("photo.webp", "Pictures")]
        public void Route_ShouldMapPicturesCorrectly(string filename, string expectedFolder)
        {
            var result = _routingEngine.GetTargetFolder(filename);
            Assert.Equal(expectedFolder, result);
        }

        [Theory]
        [InlineData("video.mp4", "Movies")]
        [InlineData("film.mkv", "Movies")]
        [InlineData("clip.mov", "Movies")]
        [InlineData("movie.avi", "Movies")]
        [InlineData("show.wmv", "Movies")]
        public void Route_ShouldMapMoviesCorrectly(string filename, string expectedFolder)
        {
            var result = _routingEngine.GetTargetFolder(filename);
            Assert.Equal(expectedFolder, result);
        }

        [Theory]
        [InlineData("song.mp3", "Audio")]
        [InlineData("sound.wav", "Audio")]
        [InlineData("track.flac", "Audio")]
        [InlineData("audio.aac", "Audio")]
        public void Route_ShouldMapAudioCorrectly(string filename, string expectedFolder)
        {
            var result = _routingEngine.GetTargetFolder(filename);
            Assert.Equal(expectedFolder, result);
        }

        [Theory]
        [InlineData("word.doc", "Documents")]
        [InlineData("word2.docx", "Documents")]
        [InlineData("text.txt", "Documents")]
        [InlineData("rich.rtf", "Documents")]
        [InlineData("excel.xlsx", "Documents")]
        [InlineData("data.csv", "Documents")]
        [InlineData("presentation.pptx", "Documents")]
        public void Route_ShouldMapDocumentsCorrectly(string filename, string expectedFolder)
        {
            var result = _routingEngine.GetTargetFolder(filename);
            Assert.Equal(expectedFolder, result);
        }

        [Theory]
        [InlineData("archive.zip", "Archives")]
        [InlineData("compressed.rar", "Archives")]
        [InlineData("stuff.7z", "Archives")]
        [InlineData("ball.tar", "Archives")]
        [InlineData("gzip.gz", "Archives")]
        public void Route_ShouldMapArchivesCorrectly(string filename, string expectedFolder)
        {
            var result = _routingEngine.GetTargetFolder(filename);
            Assert.Equal(expectedFolder, result);
        }

        [Theory]
        [InlineData("app.exe", "Executables")]
        [InlineData("installer.msi", "Executables")]
        public void Route_ShouldMapExecutablesCorrectly(string filename, string expectedFolder)
        {
            var result = _routingEngine.GetTargetFolder(filename);
            Assert.Equal(expectedFolder, result);
        }

        [Theory]
        [InlineData("script.ps1", "Other")]
        [InlineData("config.json", "Other")]
        [InlineData("unknown.xyz", "Other")]
        [InlineData("noextension", "Other")]
        [InlineData("file_with_no_ext", "Other")]
        public void Route_ShouldMapOtherCorrectly(string filename, string expectedFolder)
        {
            var result = _routingEngine.GetTargetFolder(filename);
            Assert.Equal(expectedFolder, result);
        }

        [Theory]
        [InlineData("TEST.JPG", "Pictures")]
        [InlineData("Document.TXT", "Documents")]
        public void Route_ShouldHandleCaseInsensitivity(string filename, string expectedFolder)
        {
            var result = _routingEngine.GetTargetFolder(filename);
            Assert.Equal(expectedFolder, result);
        }
    }
}
