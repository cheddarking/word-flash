using WordFlash.Core;

namespace WordFlash.Tests;

public class StartupCommandTests
{
    [Fact]
    public void QuotesPathWithSpaces() =>
        Assert.Equal(
            "\"C:\\Program Files\\Word Flash\\WordFlash.App.exe\"",
            StartupCommand.Build(@"C:\Program Files\Word Flash\WordFlash.App.exe"));

    [Theory]
    [InlineData(@"WordFlash.App.exe")]
    [InlineData(@"..\WordFlash.App.exe")]
    [InlineData(@"C:WordFlash.App.exe")]
    public void RejectsRelativePath(string path) =>
        Assert.Throws<ArgumentException>(() => StartupCommand.Build(path));

    [Theory]
    [InlineData("C:\\Apps\\Word\"Flash.exe")]
    [InlineData("C:\\Apps\\WordFlash.dll")]
    [InlineData("")]
    [InlineData("   ")]
    public void RejectsQuotesNonExeAndBlank(string path) =>
        Assert.Throws<ArgumentException>(() => StartupCommand.Build(path));

    [Fact]
    public void Matches_IgnoresCaseOfPath() =>
        Assert.True(StartupCommand.Matches("\"c:\\apps\\wordflash.app.exe\"", @"C:\Apps\WordFlash.App.exe"));

    [Fact]
    public void Matches_FalseForDifferentPathOrNull()
    {
        Assert.False(StartupCommand.Matches("\"C:\\Old\\WordFlash.App.exe\"", @"C:\Apps\WordFlash.App.exe"));
        Assert.False(StartupCommand.Matches(null, @"C:\Apps\WordFlash.App.exe"));
    }
}
