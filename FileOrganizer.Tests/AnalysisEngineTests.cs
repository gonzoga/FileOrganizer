using System;
using System.IO;
using System.Linq;
using FileOrganizer.Engines;
using FileOrganizer.Models;
using Xunit;

namespace FileOrganizer.Tests
{
    public class AnalysisEngineTests : IDisposable
    {
        private readonly string _sourceDir;
        private readonly string _destDir;
        private readonly AnalysisEngine _analysisEngine;

        public AnalysisEngineTests()
        {
            string baseDir = Path.Combine(Path.GetTempPath(), "FileOrganizer_AnalysisTests_" + Guid.NewGuid());
            _sourceDir = Path.Combine(baseDir, "Source");
            _destDir = Path.Combine(baseDir, "Destination");
            Directory.CreateDirectory(_sourceDir);
            Directory.CreateDirectory(_destDir);

            _analysisEngine = new AnalysisEngine(new RoutingEngine(), new PdfHeuristic());
        }

        [Fact]
        public void Analyze_StandardTransfer_CreatesInstruction()
        {
            string sourceFile = Path.Combine(_sourceDir, "test.jpg");
            File.WriteAllText(sourceFile, "dummy");

            var instructions = _analysisEngine.Analyze(_sourceDir, _destDir, isCopyMode: false);

            Assert.Single(instructions);
            var instruction = instructions.First();
            Assert.Equal(sourceFile, instruction.SourcePath);
            Assert.Equal(Path.Combine(_destDir, "Pictures", "test.jpg"), instruction.DestinationPath);
            Assert.Equal(ActionType.Move, instruction.ActionType);
        }

        [Fact]
        public void Analyze_StandardTransferCopyMode_CreatesCopyInstruction()
        {
            string sourceFile = Path.Combine(_sourceDir, "test.jpg");
            File.WriteAllText(sourceFile, "dummy");

            var instructions = _analysisEngine.Analyze(_sourceDir, _destDir, isCopyMode: true);

            Assert.Single(instructions);
            Assert.Equal(ActionType.Copy, instructions.First().ActionType);
        }

        [Fact]
        public void Analyze_CollisionSourceNewer_CreatesOverwriteInstruction()
        {
            string sourceFile = Path.Combine(_sourceDir, "test.jpg");
            File.WriteAllText(sourceFile, "dummy_new");
            File.SetLastWriteTime(sourceFile, DateTime.Now.AddMinutes(10));

            string destFolder = Path.Combine(_destDir, "Pictures");
            Directory.CreateDirectory(destFolder);
            string destFile = Path.Combine(destFolder, "test.jpg");
            File.WriteAllText(destFile, "dummy_old");
            File.SetLastWriteTime(destFile, DateTime.Now.AddMinutes(-10));

            var instructions = _analysisEngine.Analyze(_sourceDir, _destDir, isCopyMode: false);

            Assert.Single(instructions);
            Assert.Equal(ActionType.Overwrite, instructions.First().ActionType);
        }

        [Fact]
        public void Analyze_CollisionDestNewer_SkipsInstruction()
        {
            string sourceFile = Path.Combine(_sourceDir, "test.jpg");
            File.WriteAllText(sourceFile, "dummy_old");
            File.SetLastWriteTime(sourceFile, DateTime.Now.AddMinutes(-10));

            string destFolder = Path.Combine(_destDir, "Pictures");
            Directory.CreateDirectory(destFolder);
            string destFile = Path.Combine(destFolder, "test.jpg");
            File.WriteAllText(destFile, "dummy_new");
            File.SetLastWriteTime(destFile, DateTime.Now.AddMinutes(10));

            var instructions = _analysisEngine.Analyze(_sourceDir, _destDir, isCopyMode: false);

            Assert.Empty(instructions);
        }

        public void Dispose()
        {
            string baseDir = Directory.GetParent(_sourceDir).FullName;
            if (Directory.Exists(baseDir))
            {
                Directory.Delete(baseDir, true);
            }
        }
    }
}
