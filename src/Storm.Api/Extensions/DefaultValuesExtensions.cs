namespace Storm.Api.Extensions;

public static class DefaultValuesExtensions
{
	private static readonly Dictionary<Type, object> DEFAULT_VALUES = new();

	extension(Type type)
	{
		public object? GetDefaultValue()
		{
			if (type.IsValueType is false)
			{
				return null;
			}

			if (DEFAULT_VALUES.TryGetValue(type, out object? value))
			{
				return value;
			}

			value = Activator.CreateInstance(type);
			DEFAULT_VALUES.TryAdd(type, value!);
			return value;
		}
	}
}