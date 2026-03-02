namespace FileOrganizer.Models
{
    public enum ActionType
    {
        Move,
        Copy,
        Overwrite
    }

    public class FileTransferInstruction
    {
        public string SourcePath { get; set; } = string.Empty;
        public string DestinationPath { get; set; } = string.Empty;
        public ActionType ActionType { get; set; }
    }
}
