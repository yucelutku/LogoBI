namespace LogoBI.Engine.Tokens;

public static class TokenResolver
{
    public const int FirmWidth = 3;
    public const int PeriodWidth = 2;

    private const int MinFirm = 1;
    private const int MaxFirm = 999;
    private const int MinPeriod = 1;
    private const int MaxPeriod = 99;

    private const string TokenFirm = "{FIRMA}";
    private const string TokenPeriod = "{DONEM}";

    public static string ResolvePattern(string physicalPattern, TokenContext ctx)
    {
        ArgumentNullException.ThrowIfNull(physicalPattern);
        ArgumentNullException.ThrowIfNull(ctx);

        if (ctx.Firm < MinFirm || ctx.Firm > MaxFirm)
        {
            throw new ArgumentOutOfRangeException(nameof(ctx), $"Firm must be between {MinFirm} and {MaxFirm}.");
        }

        if (ctx.Period < MinPeriod || ctx.Period > MaxPeriod)
        {
            throw new ArgumentOutOfRangeException(nameof(ctx), $"Period must be between {MinPeriod} and {MaxPeriod}.");
        }

        string result = physicalPattern;
        if (result.Contains(TokenFirm, StringComparison.Ordinal))
        {
            result = result.Replace(TokenFirm, ctx.Firm.ToString($"D{FirmWidth}"), StringComparison.Ordinal);
        }

        if (result.Contains(TokenPeriod, StringComparison.Ordinal))
        {
            result = result.Replace(TokenPeriod, ctx.Period.ToString($"D{PeriodWidth}"), StringComparison.Ordinal);
        }

        return result;
    }
}
