using EZSee.Models;

namespace EZSee.Tests;

public class ViewerSettingsTests
{
    [Fact]
    public void DefaultSettings_HaveExpectedValues()
    {
        var settings = new ViewerSettings();
        Assert.True(settings.DarkMode);
        Assert.Equal(3.0, settings.SlideshowIntervalSeconds);
        Assert.Equal(150, settings.ThumbnailSize);
        Assert.Equal(200, settings.MaxThumbnailCacheMB);
        Assert.Equal(3, settings.PrefetchCount);
        Assert.Equal(2, settings.DecodeThreadCount);
        Assert.Null(settings.LastOpenedPath);
        Assert.Equal(1024, settings.WindowWidth);
        Assert.Equal(768, settings.WindowHeight);
        Assert.False(settings.StartMaximized);
    }

    [Fact]
    public void Load_ReturnsDefaultWhenNoFile()
    {
        var settings = ViewerSettings.Load();
        Assert.NotNull(settings);
        Assert.True(settings.DarkMode);
    }
}
