using System.Collections.Generic;
using System.IO;
using FileOrganizer.Models;

namespace FileOrganizer.Engines
{
    public class ExecutionEngine
    {
        public void Execute(List<FileTransferInstruction> instructions, bool isCopyMode = false, IProgress<int> progress = null)
        {
            int totalInstructions = instructions.Count;
            int processed = 0;

            foreach (var instruction in instructions)
            {
                string destDir = Path.GetDirectoryName(instruction.DestinationPath);
                if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                if (instruction.ActionType == ActionType.Copy)
                {
                    File.Copy(instruction.SourcePath, instruction.DestinationPath, overwrite: false);
                }
                else if (instruction.ActionType == ActionType.Move)
                {
                    File.Move(instruction.SourcePath, instruction.DestinationPath);
                }
                else if (instruction.ActionType == ActionType.Overwrite)
                {
                    if (isCopyMode)
                    {
                        File.Copy(instruction.SourcePath, instruction.DestinationPath, overwrite: true);
                    }
                    else
                    {
                        if (File.Exists(instruction.DestinationPath))
                        {
                            File.Delete(instruction.DestinationPath);
                        }
                        File.Move(instruction.SourcePath, instruction.DestinationPath);
                    }
                }

                processed++;
                if (progress != null && totalInstructions > 0)
                {
                    progress.Report((int)((processed / (double)totalInstructions) * 100));
                }
            }
        }
    }
}
