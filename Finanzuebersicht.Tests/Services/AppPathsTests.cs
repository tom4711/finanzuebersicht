using Finanzuebersicht.Services;

namespace Finanzuebersicht.Tests.Services;

public class AppPathsTests
{
    [Fact]
    public void GetDefaultDataDir_InternalBuilder_UsesMacPathWhenMacLike()
    {
        var result = AppPaths.GetDefaultDataDir("/Users/tester", "/tmp/local", isMacLike: true);

        Assert.Equal(
            Path.Combine("/Users/tester", "Library", "Application Support", "Finanzuebersicht"),
            result);
    }

    [Fact]
    public void GetDefaultDataDir_InternalBuilder_UsesLocalApplicationDataWhenNotMacLike()
    {
        var result = AppPaths.GetDefaultDataDir("/Users/tester", "/tmp/local", isMacLike: false);

        Assert.Equal(Path.Combine("/tmp/local", "Finanzuebersicht"), result);
    }

    [Fact]
    public void GetDefaultDataDir_AlwaysEndsWithFinanzuebersicht()
    {
        var result = AppPaths.GetDefaultDataDir();

        Assert.EndsWith(Path.Combine(string.Empty, "Finanzuebersicht"), result);
    }
}