namespace Storm.Api.Core.Extensions;

public static class ObjectExtensions
{
	public static void ThrowArgumentNullExceptionIfNeeded(this object arg, string paramName)
	{
		if (arg is null)
		{
			throw new ArgumentNullException(paramName);
		}
	}

	public static void ThrowArgumentNullExceptionIfNeeded(this object arg, string paramName, string message)
	{
		if (arg is null)
		{
			throw new ArgumentNullException(paramName, message);
		}
	}
}