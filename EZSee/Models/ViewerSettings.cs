using System.Text.Json;
using System.IO;

namespace EZSee.Models
{
    public class ViewerSettings
    {
        public bool DarkMode { get; set; } = true;
        public double SlideshowIntervalSeconds { get; set; } = 3.0;
        public int ThumbnailSize { get; set; } = 150;
        public int MaxThumbnailCacheMB { get; set; } = 200;
        public int PrefetchCount { get; set; } = 3;
        public int DecodeThreadCount { get; set; } = 2;
        public string? LastOpenedPath { get; set; }
        public double WindowWidth { get; set; } = 1024;
        public double WindowHeight { get; set; } = 768;
        public bool StartMaximized { get; set; } = false;

        private static string SettingsPath =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ezsee-settings.json");

        public static ViewerSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    return JsonSerializer.Deserialize<ViewerSettings>(json) ?? new ViewerSettings();
                }
            }
            catch { }
            return new ViewerSettings();
        }

        public void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsPath, json);
            }
            catch { }
        }
    }
}
