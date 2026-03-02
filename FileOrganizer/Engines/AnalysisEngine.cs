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
        private readonly MetadataEngine _metadataEngine;

        public AnalysisEngine(RoutingEngine routingEngine, PdfHeuristic pdfHeuristic, MetadataEngine metadataEngine)
        {
            _routingEngine = routingEngine;
            _pdfHeuristic = pdfHeuristic;
            _metadataEngine = metadataEngine;
        }

        public List<FileTransferInstruction> Analyze(string sourceDir, string destDir, bool isCopyMode)
        {
            var instructions = new List<FileTransferInstruction>();
            var files = Directory.EnumerateFiles(sourceDir, "*.*", SearchOption.AllDirectories);

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

                string subFolder = _metadataEngine.GetSubFolder(file);
                string destFilePath;

                if (!string.IsNullOrEmpty(subFolder))
                {
                    destFilePath = Path.Combine(destDir, targetFolder, subFolder, Path.GetFileName(file));
                }
                else
                {
                    destFilePath = Path.Combine(destDir, targetFolder, Path.GetFileName(file));
                }

                if (File.Exists(destFilePath))
                {
                    DateTime sourceTime = File.GetLastWriteTimeUtc(file);
                    DateTime destTime = File.GetLastWriteTimeUtc(destFilePath);

                    // If source is newer, overwrite
                    if (sourceTime > destTime)
                    {
                        instructions.Add(new FileTransferInstruction
                        {
                            SourcePath = file,
                            DestinationPath = destFilePath,
                            ActionType = ActionType.Overwrite
                        });
                    }
                    // Else skip
                }
                else
                {
                    instructions.Add(new FileTransferInstruction
                    {
                        SourcePath = file,
                        DestinationPath = destFilePath,
                        ActionType = isCopyMode ? ActionType.Copy : ActionType.Move
                    });
                }

            }

            return instructions;
        }
    }
}
