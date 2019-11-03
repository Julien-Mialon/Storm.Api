using System.Runtime.CompilerServices;

namespace Storm.Api.Core.Validations
{
	public static class ObjectValidations
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsNotNull(this object o) => o != null;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsNull(this object o) => o is null;
	}
}