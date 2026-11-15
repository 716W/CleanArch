namespace GymManagement.Application.Contracts.Identity;

public sealed class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string Key { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public double DurationInMinutes { get; init; }
}