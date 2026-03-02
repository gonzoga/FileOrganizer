using System.Collections.Generic;
using System.IO;
using FileOrganizer.Models;

namespace FileOrganizer.Engines
{
    public class ExecutionEngine
    {
        public void Execute(List<FileTransferInstruction> instructions, bool isCopyMode = false, IProgress<int>? progress = null)
        {
            int totalInstructions = instructions.Count;
            int processed = 0;

            foreach (var instruction in instructions)
            {
                string? destDir = Path.GetDirectoryName(instruction.DestinationPath);
                if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                try
                {
                    bool isCopyOp = isCopyMode || instruction.ActionType == ActionType.Copy;
                    
                    if (File.Exists(instruction.DestinationPath))
                    {
                        DateTime sourceTime = File.GetLastWriteTimeUtc(instruction.SourcePath);
                        DateTime destTime = File.GetLastWriteTimeUtc(instruction.DestinationPath);

                        if (sourceTime > destTime || instruction.ActionType == ActionType.Overwrite)
                        {
                            if (isCopyOp)
                            {
                                File.Copy(instruction.SourcePath, instruction.DestinationPath, overwrite: true);
                            }
                            else
                            {
                                File.Delete(instruction.DestinationPath);
                                File.Move(instruction.SourcePath, instruction.DestinationPath);
                            }
                        }
                    }
                    else
                    {
                        if (isCopyOp)
                        {
                            File.Copy(instruction.SourcePath, instruction.DestinationPath, overwrite: false);
                        }
                        else
                        {
                            File.Move(instruction.SourcePath, instruction.DestinationPath);
                        }
                    }
                }
                catch (System.Exception)
                {
                    // Gracefully swallow unauthorized access, IO locks, or other file-level exceptions
                    // to ensure the loop continues processing the rest of the plan.
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
