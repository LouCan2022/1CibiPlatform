namespace FrontendWebassembly.SharedService;

public static class OrderTypeDisplay
{
	// Mirrors BackendAPI/Modules/ATS/Constants/OrderType.cs
	public const string Normal = "Normal";
	public const string Rush = "Rush";

	public static string GetText(string? orderType)
		=> string.IsNullOrWhiteSpace(orderType) ? "—" : orderType.Trim();

	public static string GetClass(string? orderType)
		=> Is(orderType, Rush) ? "is-rush" : "is-normal";

	private static bool Is(string? a, string b)
		=> string.Equals(a?.Trim(), b, StringComparison.OrdinalIgnoreCase);
}
