using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace EZSee.Services
{
    public class ImageService
    {
        private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".tif", ".webp", ".heif", ".heic", ".ico"
        };

        public static bool IsSupported(string filePath)
        {
            return SupportedExtensions.Contains(Path.GetExtension(filePath));
        }

        public static List<string> GetImagesInFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                return new List<string>();

            return Directory.EnumerateFiles(folderPath)
                .Where(f => SupportedExtensions.Contains(Path.GetExtension(f)))
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static BitmapImage? LoadImage(string filePath, int? decodePixelWidth = null)
        {
            try
            {
                if (!File.Exists(filePath))
                    return null;

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
                if (decodePixelWidth.HasValue)
                    bitmap.DecodePixelWidth = decodePixelWidth.Value;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        public static Models.ImageInfo GetImageInfo(string filePath)
        {
            var info = new Models.ImageInfo
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                Format = Path.GetExtension(filePath).TrimStart('.').ToUpperInvariant()
            };

            try
            {
                var fileInfo = new FileInfo(filePath);
                info.FileSizeBytes = fileInfo.Length;
                info.DateModified = fileInfo.LastWriteTime;

                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
                if (decoder.Frames.Count > 0)
                {
                    info.Width = decoder.Frames[0].PixelWidth;
                    info.Height = decoder.Frames[0].PixelHeight;
                }
            }
            catch { }

            return info;
        }
    }
}
