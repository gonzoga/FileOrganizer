using System;
using System.Collections.Generic;
using System.IO;
using FileOrganizer.Models;

namespace FileOrganizer.Engines
{
    public class AnalysisEngine
    {
        private readonly RoutingEngine _routingEngine;
        private readonly PdfHeuristic _pdfHeuristic;

        public AnalysisEngine(RoutingEngine routingEngine, PdfHeuristic pdfHeuristic)
        {
            _routingEngine = routingEngine;
            _pdfHeuristic = pdfHeuristic;
        }

        public async IAsyncEnumerable<FileTransferInstruction> AnalyzeAsync(string sourceDir, string destDir, bool isCopyMode, IProgress<int> progress = null)
        {
            var files = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);
            int totalFiles = files.Length;
            int processed = 0;

            foreach (var file in files)
            {
                string extension = Path.GetExtension(file)?.ToLowerInvariant() ?? "";
                string targetFolder;

                if (extension == ".pdf" || extension == ".epub" || extension == ".mobi")
                {
                    targetFolder = _pdfHeuristic.GetBookOrPdfFolder(file);
                }
                else
                {
                    targetFolder = _routingEngine.GetTargetFolder(file);
                }

                string relativePath = Path.GetRelativePath(sourceDir, file);
                string destFilePath = Path.Combine(destDir, targetFolder, relativePath);

                if (File.Exists(destFilePath))
                {
                    DateTime sourceTime = File.GetLastWriteTimeUtc(file);
                    DateTime destTime = File.GetLastWriteTimeUtc(destFilePath);

                    // If source is newer, overwrite
                    if (sourceTime > destTime)
                    {
                        yield return new FileTransferInstruction
                        {
                            SourcePath = file,
                            DestinationPath = destFilePath,
                            ActionType = ActionType.Overwrite
                        };
                    }
                    // Else skip
                }
                else
                {
                    yield return new FileTransferInstruction
                    {
                        SourcePath = file,
                        DestinationPath = destFilePath,
                        ActionType = isCopyMode ? ActionType.Copy : ActionType.Move
                    };
                }

                processed++;
                if (progress != null && totalFiles > 0)
                {
                    progress.Report((int)((processed / (double)totalFiles) * 100));
                }
            }
        }
    }
}
