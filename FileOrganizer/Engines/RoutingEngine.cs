using System;
using System.Collections.Generic;
using System.IO;

namespace FileOrganizer.Engines
{
    public class RoutingEngine
    {
        private static readonly Dictionary<string, string> ExtensionMap = new(StringComparer.OrdinalIgnoreCase)
        {
            // Pictures
            { ".jpg", "Pictures" }, { ".jpeg", "Pictures" }, { ".png", "Pictures" },
            { ".gif", "Pictures" }, { ".bmp", "Pictures" }, { ".webp", "Pictures" },
            // Movies
            { ".mp4", "Movies" }, { ".mkv", "Movies" }, { ".mov", "Movies" },
            { ".avi", "Movies" }, { ".wmv", "Movies" },
            // Audio
            { ".mp3", "Audio" }, { ".wav", "Audio" }, { ".flac", "Audio" }, { ".aac", "Audio" },
            { ".wma", "Audio" }, { ".m4a", "Audio" }, { ".ogg", "Audio" }, { ".m4b", "Audio" },
            // Documents
            { ".doc", "Documents" }, { ".docx", "Documents" }, { ".txt", "Documents" },
            { ".rtf", "Documents" }, { ".xlsx", "Documents" }, { ".csv", "Documents" },
            { ".pptx", "Documents" },
            // Archives
            { ".zip", "Archives" }, { ".rar", "Archives" }, { ".7z", "Archives" },
            { ".tar", "Archives" }, { ".gz", "Archives" },
            // Executables
            { ".exe", "Executables" }, { ".msi", "Executables" }
        };

        public string GetTargetFolder(string filepath)
        {
            string? extension = Path.GetExtension(filepath);
            
            if (string.IsNullOrEmpty(extension))
                return "Other";

            if (ExtensionMap.TryGetValue(extension, out string? folder))
            {
                return folder ?? "Other";
            }

            return "Other";
        }
    }
}
