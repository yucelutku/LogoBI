using LogoBI.Engine.Tokens;
using Xunit;

namespace LogoBI.Engine.Tests;

public class TokenResolverTests
{
    [Fact]
    public void ResolvePattern_ReplacesFirmAndPeriodCorrectly()
    {
        var ctx = new TokenContext { Firm = 9, Period = 1 };
        string pattern = "LG_{FIRMA}_{DONEM}_INVOICE";

        string resolved = TokenResolver.ResolvePattern(pattern, ctx);

        Assert.Equal("LG_009_01_INVOICE", resolved);
    }

    [Fact]
    public void ResolvePattern_FirmOnlyTable_DoesNotContainPeriodToken()
    {
        var ctx = new TokenContext { Firm = 12, Period = 5 };
        string pattern = "LG_{FIRMA}_CLCARD";

        string resolved = TokenResolver.ResolvePattern(pattern, ctx);

        Assert.Equal("LG_012_CLCARD", resolved);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1000)]
    public void ResolvePattern_InvalidFirm_ThrowsArgumentOutOfRangeException(int firm)
    {
        var ctx = new TokenContext { Firm = firm, Period = 1 };
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            TokenResolver.ResolvePattern("LG_{FIRMA}_INVOICE", ctx));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void ResolvePattern_InvalidPeriod_ThrowsArgumentOutOfRangeException(int period)
    {
        var ctx = new TokenContext { Firm = 1, Period = period };
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            TokenResolver.ResolvePattern("LG_{FIRMA}_{DONEM}_INVOICE", ctx));
    }
}
