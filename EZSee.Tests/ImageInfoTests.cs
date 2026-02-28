using EZSee.Models;

namespace EZSee.Tests;

public class ImageInfoTests
{
    [Fact]
    public void FileSizeDisplay_Bytes()
    {
        var info = new ImageInfo { FileSizeBytes = 500 };
        Assert.Equal("500 B", info.FileSizeDisplay);
    }

    [Fact]
    public void FileSizeDisplay_Kilobytes()
    {
        var info = new ImageInfo { FileSizeBytes = 2048 };
        Assert.Equal("2.0 KB", info.FileSizeDisplay);
    }

    [Fact]
    public void FileSizeDisplay_Megabytes()
    {
        var info = new ImageInfo { FileSizeBytes = 3 * 1024 * 1024 };
        Assert.Equal("3.0 MB", info.FileSizeDisplay);
    }

    [Fact]
    public void DimensionsDisplay_Format()
    {
        var info = new ImageInfo { Width = 1920, Height = 1080 };
        Assert.Equal("1920 × 1080", info.DimensionsDisplay);
    }

    [Fact]
    public void DefaultValues()
    {
        var info = new ImageInfo();
        Assert.Equal(string.Empty, info.FilePath);
        Assert.Equal(string.Empty, info.FileName);
        Assert.Equal(string.Empty, info.Format);
        Assert.Equal(0, info.Width);
        Assert.Equal(0, info.Height);
    }
}
