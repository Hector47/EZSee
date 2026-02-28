using System;

namespace EZSee.Models
{
    public class ImageInfo
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
        public long FileSizeBytes { get; set; }
        public DateTime DateModified { get; set; }
        public string Format { get; set; } = string.Empty;

        public string FileSizeDisplay
        {
            get
            {
                if (FileSizeBytes < 1024) return $"{FileSizeBytes} B";
                if (FileSizeBytes < 1024 * 1024) return $"{FileSizeBytes / 1024.0:F1} KB";
                return $"{FileSizeBytes / (1024.0 * 1024.0):F1} MB";
            }
        }

        public string DimensionsDisplay => $"{Width} × {Height}";
    }
}
