using System;
using System.Collections.Generic;
using System.IO;
using FileOrganizer.Engines;
using FileOrganizer.Models;
using Xunit;

namespace FileOrganizer.Tests
{
    public class ExecutionEngineTests : IDisposable
    {
        private readonly string _sourceDir;
        private readonly string _destDir;
        private readonly ExecutionEngine _executionEngine;

        public ExecutionEngineTests()
        {
            string baseDir = Path.Combine(Path.GetTempPath(), "FileOrganizer_ExecutionTests_" + Guid.NewGuid());
            _sourceDir = Path.Combine(baseDir, "Source");
            _destDir = Path.Combine(baseDir, "Destination");
            Directory.CreateDirectory(_sourceDir);
            Directory.CreateDirectory(_destDir);

            _executionEngine = new ExecutionEngine();
        }

        [Fact]
        public void Execute_MoveInstruction_CreatesFolderAndMovesFile()
        {
            string sourceFile = Path.Combine(_sourceDir, "test.txt");
            File.WriteAllText(sourceFile, "dummy");
            string destFile = Path.Combine(_destDir, "Documents", "test.txt");

            var instructions = new List<FileTransferInstruction>
            {
                new FileTransferInstruction { SourcePath = sourceFile, DestinationPath = destFile, ActionType = ActionType.Move }
            };

            _executionEngine.Execute(instructions);

            Assert.False(File.Exists(sourceFile));
            Assert.True(File.Exists(destFile));
        }

        [Fact]
        public void Execute_CopyInstruction_CreatesFolderAndCopiesFile()
        {
            string sourceFile = Path.Combine(_sourceDir, "copy.txt");
            File.WriteAllText(sourceFile, "dummy");
            string destFile = Path.Combine(_destDir, "Documents", "copy.txt");

            var instructions = new List<FileTransferInstruction>
            {
                new FileTransferInstruction { SourcePath = sourceFile, DestinationPath = destFile, ActionType = ActionType.Copy }
            };

            _executionEngine.Execute(instructions);

            Assert.True(File.Exists(sourceFile));
            Assert.True(File.Exists(destFile));
        }

        [Fact]
        public void Execute_OverwriteInstruction_OverwritesDestinationFile()
        {
            string sourceFile = Path.Combine(_sourceDir, "overwrite.txt");
            File.WriteAllText(sourceFile, "new_dummy");
            
            string destDirSub = Path.Combine(_destDir, "Documents");
            Directory.CreateDirectory(destDirSub);
            string destFile = Path.Combine(destDirSub, "overwrite.txt");
            File.WriteAllText(destFile, "old_dummy");

            var instructions = new List<FileTransferInstruction>
            {
                new FileTransferInstruction { SourcePath = sourceFile, DestinationPath = destFile, ActionType = ActionType.Overwrite }
            };

            _executionEngine.Execute(instructions);

            Assert.False(File.Exists(sourceFile), "Source file should be moved/deleted when overwriting (since default operation seems to be Move).");
            Assert.True(File.Exists(destFile));
            Assert.Equal("new_dummy", File.ReadAllText(destFile));
        }

        [Fact]
        public void Execute_OverwriteInstructionInCopyMode_OverwritesDestinationFileAndKeepsSource()
        {
            string sourceFile = Path.Combine(_sourceDir, "overwrite_copy.txt");
            File.WriteAllText(sourceFile, "new_dummy");
            
            string destDirSub = Path.Combine(_destDir, "Documents");
            Directory.CreateDirectory(destDirSub);
            string destFile = Path.Combine(destDirSub, "overwrite_copy.txt");
            File.WriteAllText(destFile, "old_dummy");

            var instructions = new List<FileTransferInstruction>
            {
                new FileTransferInstruction { SourcePath = sourceFile, DestinationPath = destFile, ActionType = ActionType.Overwrite }
            };

            _executionEngine.Execute(instructions, isCopyMode: true);

            Assert.True(File.Exists(sourceFile), "Source file should be kept when overwriting in Copy mode.");
            Assert.True(File.Exists(destFile));
            Assert.Equal("new_dummy", File.ReadAllText(destFile));
        }

        [Fact]
        public void Execute_CollisionDuringExecution_SkipsOlderSource()
        {
            string sourceFile = Path.Combine(_sourceDir, "collide.txt");
            File.WriteAllText(sourceFile, "old_dummy");
            File.SetLastWriteTimeUtc(sourceFile, DateTime.UtcNow.AddMinutes(-10));

            string destDirSub = Path.Combine(_destDir, "Documents");
            Directory.CreateDirectory(destDirSub);
            string destFile = Path.Combine(destDirSub, "collide.txt");
            File.WriteAllText(destFile, "new_dummy");
            File.SetLastWriteTimeUtc(destFile, DateTime.UtcNow.AddMinutes(10));

            var instructions = new List<FileTransferInstruction>
            {
                new FileTransferInstruction { SourcePath = sourceFile, DestinationPath = destFile, ActionType = ActionType.Move } // Move, but destination exists
            };

            _executionEngine.Execute(instructions);

            // Source shouldn't be moved because its dest is newer
            Assert.True(File.Exists(sourceFile));
            Assert.Equal("new_dummy", File.ReadAllText(destFile));
        }

        [Fact]
        public void Execute_CollisionDuringExecution_OverwritesOlderDest()
        {
            string sourceFile = Path.Combine(_sourceDir, "collide2.txt");
            File.WriteAllText(sourceFile, "new_dummy");
            File.SetLastWriteTimeUtc(sourceFile, DateTime.UtcNow.AddMinutes(10));

            string destDirSub = Path.Combine(_destDir, "Documents");
            Directory.CreateDirectory(destDirSub);
            string destFile = Path.Combine(destDirSub, "collide2.txt");
            File.WriteAllText(destFile, "old_dummy");
            File.SetLastWriteTimeUtc(destFile, DateTime.UtcNow.AddMinutes(-10));

            var instructions = new List<FileTransferInstruction>
            {
                new FileTransferInstruction { SourcePath = sourceFile, DestinationPath = destFile, ActionType = ActionType.Move } // Move, despite destination existing
            };

            _executionEngine.Execute(instructions);

            // Source should be moved
            Assert.False(File.Exists(sourceFile));
            Assert.Equal("new_dummy", File.ReadAllText(destFile));
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
