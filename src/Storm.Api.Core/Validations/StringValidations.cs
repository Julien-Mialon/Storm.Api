using System.Runtime.CompilerServices;

namespace Storm.Api.Core.Validations;

public static class StringValidations
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNullOrEmpty(this string s) => string.IsNullOrEmpty(s);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNotNullOrEmpty(this string s) => !string.IsNullOrEmpty(s);
}