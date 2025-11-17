namespace Storm.Api.Extensions;

public static class ParseExtensions
{
	public static Guid? ToGuid(this string value)
	{
		if (Guid.TryParse(value, out Guid result))
		{
			return result;
		}

		return null;
	}
}