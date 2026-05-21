namespace Storm.Api.Extensions;

public static class DefaultValuesExtensions
{
	private static Dictionary<Type, object> _defaultValues = new();

	extension(Type type)
	{
		public object? GetDefaultValue()
		{
			if (type.IsValueType is false)
			{
				return null;
			}

			if (_defaultValues.TryGetValue(type, out object? value))
			{
				return value;
			}

			value = Activator.CreateInstance(type);

			Dictionary<Type, object> snapshot, newCache;
			do
			{
				snapshot = _defaultValues;
				newCache = new(_defaultValues)
				{
					[type] = value!
				};
			} while (!ReferenceEquals(Interlocked.CompareExchange(ref _defaultValues, newCache, snapshot), snapshot));


			return value;
		}
	}
}