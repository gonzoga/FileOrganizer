using System;
using System.IO;
using FileOrganizer.Engines;
using PdfSharp.Pdf;
using Xunit;

namespace FileOrganizer.Tests
{
    public class PdfHeuristicTests : IDisposable
    {
        private readonly string _tempDirectory;
        private readonly PdfHeuristic _pdfHeuristic;

        public PdfHeuristicTests()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), "FileOrganizer_PdfTests_" + Guid.NewGuid());
            Directory.CreateDirectory(_tempDirectory);
            _pdfHeuristic = new PdfHeuristic();
        }

        private string CreateMockPdf(string filename, int pageCount)
        {
            string path = Path.Combine(_tempDirectory, filename);
            using (var document = new PdfDocument())
            {
                for (int i = 0; i < pageCount; i++)
                {
                    document.AddPage();
                }
                document.Save(path);
            }
            return path;
        }

        private string CreateCorruptPdf(string filename)
        {
            string path = Path.Combine(_tempDirectory, filename);
            File.WriteAllText(path, "This is not a real PDF file, just garbage text.");
            return path;
        }

        [Fact]
        public void GetBookOrPdfFolder_MapsEpubToBooks()
        {
            var result = _pdfHeuristic.GetBookOrPdfFolder("test.epub");
            Assert.Equal("Books", result);
        }

        [Fact]
        public void GetBookOrPdfFolder_MapsMobiToBooks()
        {
            var result = _pdfHeuristic.GetBookOrPdfFolder("test.mobi");
            Assert.Equal("Books", result);
        }

        [Fact]
        public void GetBookOrPdfFolder_MapsSmallPdfToPDFs()
        {
            string path = CreateMockPdf("small.pdf", 5);
            var result = _pdfHeuristic.GetBookOrPdfFolder(path);
            Assert.Equal("PDFs", result);
        }

        [Fact]
        public void GetBookOrPdfFolder_MapsLargePdfToBooks()
        {
            string path = CreateMockPdf("large.pdf", 31);
            var result = _pdfHeuristic.GetBookOrPdfFolder(path);
            Assert.Equal("Books", result);
        }

        [Fact]
        public void GetBookOrPdfFolder_MapsCorruptPdfToPDFs()
        {
            string path = CreateCorruptPdf("corrupt.pdf");
            var result = _pdfHeuristic.GetBookOrPdfFolder(path);
            Assert.Equal("PDFs", result);
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
