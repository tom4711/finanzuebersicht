using Finanzuebersicht.ViewModels;

namespace Finanzuebersicht.Tests.ViewModels.Settings;

public class AboutViewModelTests
{
    [Fact]
    public void Constructor_SetsAppVersionAndBuildInfo()
    {
        var sut = new AboutViewModel();

        Assert.False(string.IsNullOrWhiteSpace(sut.AppVersion));
        Assert.NotNull(sut.BuildInfo);
    }

    [Fact]
    public void Libraries_IsNotNull()
    {
        var sut = new AboutViewModel();

        Assert.NotNull(sut.Libraries);
    }
}
