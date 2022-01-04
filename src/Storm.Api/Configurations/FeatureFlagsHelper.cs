using Microsoft.Extensions.Configuration;
using Storm.Api.Core.Features;

namespace Storm.Api.Configurations;

public static class FeatureFlagsHelper
{
	public static void Load(IConfiguration configuration)
	{
		Dictionary<string, bool>? flags = configuration.GetChildren()
			.ToDictionary(x => x.Key, x => x.Get<bool>());

		FeatureFlags.Set(flags);
	}
}