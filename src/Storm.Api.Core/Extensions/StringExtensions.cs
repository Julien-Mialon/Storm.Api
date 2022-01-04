using System.Runtime.CompilerServices;

namespace Storm.Api.Core.Extensions;

public static class StringExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNullOrEmpty(this string s) => string.IsNullOrEmpty(s);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool NotNullOrEmpty(this string s) => !string.IsNullOrEmpty(s);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string ValueIfNull(this string s, string value) => s ?? value;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string? NullIfEmpty(this string source) => string.IsNullOrEmpty(source) ? null : source;
}