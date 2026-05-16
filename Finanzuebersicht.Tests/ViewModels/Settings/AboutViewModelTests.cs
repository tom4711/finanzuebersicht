using System.Reflection;
using System.Reflection.Emit;
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
    public void AppVersion_WithPlusSuffix_ExtractsVersionBeforePlus()
    {
        var assembly = CreateAssemblyWithInformationalVersion("1.2.3+abc");
        var sut = new AboutViewModel(assembly);

        Assert.Equal("1.2.3", sut.AppVersion);
        Assert.Equal("abc", sut.BuildInfo);
    }

    [Fact]
    public void BuildInfo_WithPlusSuffix_ExtractsBuildInfoAfterPlus()
    {
        var assembly = CreateAssemblyWithInformationalVersion("1.2.3+abc");
        var sut = new AboutViewModel(assembly);

        Assert.Equal("1.2.3+abc", $"{sut.AppVersion}+{sut.BuildInfo}");
    }

    [Fact]
    public void Libraries_IsNotNull()
    {
        var sut = new AboutViewModel();

        Assert.NotNull(sut.Libraries);
    }

    private static Assembly CreateAssemblyWithInformationalVersion(string informationalVersion)
    {
        var assemblyName = new AssemblyName($"AboutViewModelTests_{Guid.NewGuid():N}");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var constructor = typeof(AssemblyInformationalVersionAttribute).GetConstructor([typeof(string)])!;
        var attributeBuilder = new CustomAttributeBuilder(constructor, [informationalVersion]);
        assemblyBuilder.SetCustomAttribute(attributeBuilder);
        return assemblyBuilder;
    }
}
