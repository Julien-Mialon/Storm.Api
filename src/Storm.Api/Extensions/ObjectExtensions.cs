using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Storm.Api.Extensions;

public static class ObjectExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNotNull([NotNullWhen(true)] this object? o) => o != null;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNull([NotNullWhen(false)] this object? o) => o is null;
}