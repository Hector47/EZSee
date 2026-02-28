using System.IO;
using EZSee.Services;

namespace EZSee.Tests;

public class ImageServiceTests
{
    [Theory]
    [InlineData("test.jpg", true)]
    [InlineData("test.jpeg", true)]
    [InlineData("test.png", true)]
    [InlineData("test.gif", true)]
    [InlineData("test.bmp", true)]
    [InlineData("test.tiff", true)]
    [InlineData("test.tif", true)]
    [InlineData("test.webp", true)]
    [InlineData("test.ico", true)]
    [InlineData("test.JPG", true)]
    [InlineData("test.PNG", true)]
    [InlineData("test.txt", false)]
    [InlineData("test.pdf", false)]
    [InlineData("test.doc", false)]
    [InlineData("test", false)]
    public void IsSupported_ReturnsCorrectResult(string filePath, bool expected)
    {
        Assert.Equal(expected, ImageService.IsSupported(filePath));
    }

    [Fact]
    public void GetImagesInFolder_ReturnsEmpty_ForNonExistentFolder()
    {
        var result = ImageService.GetImagesInFolder("/nonexistent/folder");
        Assert.Empty(result);
    }

    [Fact]
    public void GetImagesInFolder_ReturnsEmpty_ForEmptyFolder()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            var result = ImageService.GetImagesInFolder(tempDir);
            Assert.Empty(result);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void GetImagesInFolder_FiltersNonImageFiles()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "readme.txt"), "text");
            File.WriteAllText(Path.Combine(tempDir, "data.csv"), "data");
            var result = ImageService.GetImagesInFolder(tempDir);
            Assert.Empty(result);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void LoadImage_ReturnsNull_ForNonExistentFile()
    {
        var result = ImageService.LoadImage("/nonexistent/file.jpg");
        Assert.Null(result);
    }
}
