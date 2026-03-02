using System;
using System.IO;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace FileOrganizer.Engines
{
    public class PdfHeuristic
    {
        public string GetBookOrPdfFolder(string filepath)
        {
            try
            {
                string? extension = Path.GetExtension(filepath)?.ToLowerInvariant();

                if (extension == ".epub" || extension == ".mobi")
                {
                    return "Books";
                }

                if (extension == ".pdf")
                {
                    using (PdfDocument document = PdfReader.Open(filepath, PdfDocumentOpenMode.Import))
                    {
                        if (document.PageCount > 30)
                        {
                            return "Books";
                        }
                    }
                }
            }
            catch
            {
                // If any error occurs reading the PDF (corrupt, etc.), default to PDFs
                return "PDFs";
            }

            return "PDFs";
        }
    }
}
