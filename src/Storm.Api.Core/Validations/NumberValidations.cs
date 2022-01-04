using System.Runtime.CompilerServices;

namespace Storm.Api.Core.Validations;

public static class NumberValidations
{
	// int
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsPositive(this int value) => value >= 0;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsStrictlyPositive(this int value) => value > 0;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNegative(this int value) => value <= 0;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsStrictlyNegative(this int value) => value < 0;

	// int?
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNotNull(this int? value) => value.HasValue;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNull(this int? value) => !value.HasValue;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsPositive(this int? value) => value.HasValue && value >= 0;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsStrictlyPositive(this int? value) => value.HasValue && value > 0;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNegative(this int? value) => value.HasValue && value <= 0;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsStrictlyNegative(this int? value) => value.HasValue && value < 0;

	// long
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsPositive(this long value) => value >= 0;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsStrictlyPositive(this long value) => value > 0;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNegative(this long value) => value <= 0;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsStrictlyNegative(this long value) => value < 0;

	//long?
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNotNull(this long? value) => value.HasValue;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNull(this long? value) => !value.HasValue;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsPositive(this long? value) => value.HasValue && value >= 0;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsStrictlyPositive(this long? value) => value.HasValue && value > 0;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNegative(this long? value) => value.HasValue && value <= 0;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsStrictlyNegative(this long? value) => value.HasValue && value < 0;

	//float
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsPositive(this float value) => value >= 0f;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsStrictlyPositive(this float value) => value > 0f;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNegative(this float value) => value <= 0f;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsStrictlyNegative(this float value) => value < 0f;

	//float?
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNotNull(this float? value) => value.HasValue;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNull(this float? value) => !value.HasValue;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsPositive(this float? value) => value.HasValue && value >= 0f;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsStrictlyPositive(this float? value) => value.HasValue && value > 0f;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNegative(this float? value) => value.HasValue && value <= 0f;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsStrictlyNegative(this float? value) => value.HasValue && value < 0f;

	//double
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsPositive(this double value) => value >= 0.0;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsStrictlyPositive(this double value) => value > 0.0;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNegative(this double value) => value <= 0.0;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsStrictlyNegative(this double value) => value < 0.0;

	//double?
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNotNull(this double? value) => value.HasValue;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNull(this double? value) => !value.HasValue;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsPositive(this double? value) => value.HasValue && value >= 0.0;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsStrictlyPositive(this double? value) => value.HasValue && value > 0.0;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNegative(this double? value) => value.HasValue && value <= 0.0;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsStrictlyNegative(this double? value) => value.HasValue && value < 0.0;

	//decimal
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsPositive(this decimal value) => value >= 0m;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsStrictlyPositive(this decimal value) => value > 0m;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNegative(this decimal value) => value <= 0m;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsStrictlyNegative(this decimal value) => value < 0m;

	//decimal?
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNotNull(this decimal? value) => value.HasValue;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNull(this decimal? value) => !value.HasValue;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsPositive(this decimal? value) => value.HasValue && value >= 0m;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsStrictlyPositive(this decimal? value) => value.HasValue && value > 0m;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNegative(this decimal? value) => value.HasValue && value <= 0m;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsStrictlyNegative(this decimal? value) => value.HasValue && value < 0m;
}