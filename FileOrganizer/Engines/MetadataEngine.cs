using System;
using System.IO;
using System.Linq;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Jpeg;
using MetadataExtractor.Formats.Png;

namespace FileOrganizer.Engines
{
    public class MetadataEngine
    {
        public string GetSubFolder(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return "";

            string extension = Path.GetExtension(filePath)?.ToLowerInvariant() ?? "";

            try
            {
                if (IsAudioExtension(extension))
                {
                    return GetAudioSubFolder(filePath);
                }
                
                if (IsImageExtension(extension))
                {
                    return GetImageSubFolder(filePath);
                }
            }
            catch (Exception)
            {
                // Silently swallow parsing errors (e.g. corrupt tags) to gracefully fallback to root category
            }

            return "";
        }

        private bool IsAudioExtension(string ext)
        {
            return ext == ".mp3" || ext == ".wav" || ext == ".flac" || ext == ".m4a" || ext == ".aac" || ext == ".wma" || ext == ".ogg" || ext == ".m4b";
        }

        private string GetAudioSubFolder(string filePath)
        {
            try
            {
                using (var tfile = TagLib.File.Create(filePath))
                {
                    string artist = tfile.Tag.FirstPerformer ?? tfile.Tag.FirstAlbumArtist ?? "";
                    
                    if (!string.IsNullOrWhiteSpace(artist))
                    {
                        // Clean artist name of invalid path characters
                        return SanitizeForPath(artist);
                    }
                }
            }
            catch (TagLib.UnsupportedFormatException)
            {
            }
            catch (TagLib.CorruptFileException)
            {
            }
            return "Unknown Artist";
        }

        private bool IsImageExtension(string ext)
        {
            return ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".cr2" || ext == ".nef" || ext == ".arw";
        }

        private string GetImageSubFolder(string filePath)
        {
            try
            {
                var directories = ImageMetadataReader.ReadMetadata(filePath);
                
                // 1. Try to find EXIF Camera Data first
                var ifd0Directory = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
                if (ifd0Directory != null)
                {
                    string? make = ifd0Directory.GetString(ExifDirectoryBase.TagMake);
                    string? model = ifd0Directory.GetString(ExifDirectoryBase.TagModel);

                    if (!string.IsNullOrWhiteSpace(model))
                        return SanitizeForPath(model.Trim());
                        
                    if (!string.IsNullOrWhiteSpace(make))
                        return SanitizeForPath(make.Trim());
                }

                // 2. If no EXIF data, check image dimensions to heuristic sort
                int width = 0;
                int height = 0;

                // Check JPEG dimensions
                var jpegDir = directories.OfType<JpegDirectory>().FirstOrDefault();
                if (jpegDir != null)
                {
                    width = jpegDir.GetInt32(JpegDirectory.TagImageWidth);
                    height = jpegDir.GetInt32(JpegDirectory.TagImageHeight);
                }
                else
                {
                    // Check PNG dimensions
                    var pngDir = directories.OfType<PngDirectory>().FirstOrDefault();
                    if (pngDir != null)
                    {
                        width = pngDir.GetInt32(PngDirectory.TagImageWidth);
                        height = pngDir.GetInt32(PngDirectory.TagImageHeight);
                    }
                }

                if (width > 0 && height > 0)
                {
                    if (width < 512 && height < 512)
                    {
                        return "Icons & Graphics";
                    }
                }
            }
            catch (Exception)
            {
            }
            return "Unsorted Photos";
        }

        private string SanitizeForPath(string folderName)
        {
            folderName = folderName.Trim();
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                folderName = folderName.Replace(c.ToString(), "");
            }
            return folderName.Length > 0 ? folderName : "Unknown";
        }
    }
}
