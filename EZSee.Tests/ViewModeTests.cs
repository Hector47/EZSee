using EZSee.Models;

namespace EZSee.Tests;

public class ViewModeTests
{
    [Fact]
    public void ViewMode_HasExpectedValues()
    {
        Assert.Equal(0, (int)ViewMode.SingleImage);
        Assert.Equal(1, (int)ViewMode.FolderView);
    }

    [Fact]
    public void ViewMode_CanParseFromString()
    {
        Assert.True(Enum.TryParse<ViewMode>("SingleImage", out var mode));
        Assert.Equal(ViewMode.SingleImage, mode);
    }
}
