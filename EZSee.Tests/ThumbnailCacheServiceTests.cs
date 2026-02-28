using EZSee.Services;

namespace EZSee.Tests;

public class ThumbnailCacheServiceTests
{
    [Fact]
    public void GetCached_ReturnsNull_WhenEmpty()
    {
        using var cache = new ThumbnailCacheService();
        Assert.Null(cache.GetCached("test.jpg"));
    }

    [Fact]
    public void Clear_DoesNotThrow()
    {
        using var cache = new ThumbnailCacheService();
        cache.Clear(); // Should not throw
    }

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        var cache = new ThumbnailCacheService();
        cache.Dispose(); // Should not throw
    }

    [Fact]
    public void Constructor_AcceptsCustomConcurrency()
    {
        using var cache = new ThumbnailCacheService(4);
        Assert.Null(cache.GetCached("anything"));
    }
}
