using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Storm.Api.Extensions;

public static class StringExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNullOrEmpty([NotNullWhen(false)] this string? s) => string.IsNullOrEmpty(s);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? input) => string.IsNullOrWhiteSpace(input);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNotNullOrEmpty([NotNullWhen(true)] this string? s) => !string.IsNullOrEmpty(s);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNotNullOrWhiteSpace([NotNullWhen(true)] this string? input) => !string.IsNullOrWhiteSpace(input);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string ValueIfNull(this string? s, string value) => s ?? value;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string ValueIfNullOrEmpty(this string? s, string value) => string.IsNullOrEmpty(s) ? value : s;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string ValueIfNullOrWhiteSpace(this string? s, string value) => string.IsNullOrEmpty(s) ? value : s;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string? NullIfEmpty(this string source) => string.IsNullOrEmpty(source) ? null : source;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string? NullIfWhiteSpace(this string source) => string.IsNullOrWhiteSpace(source) ? null : source;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string OrEmpty(this string? source) => source ?? string.Empty;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int AsInt(this string? input) => int.TryParse(input ?? string.Empty, out int result) ? result : 0;
}