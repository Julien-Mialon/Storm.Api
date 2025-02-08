using Microsoft.Extensions.Configuration;

namespace Storm.Api.Vaults;

public static class VaultExtensions
{
	public static IConfigurationBuilder AddVault(this IConfigurationBuilder configuration, VaultConfiguration vaultConfiguration)
	{
		return configuration.Add(new VaultConfigurationSource(vaultConfiguration));
	}

	public static VaultConfiguration LoadVaultConfiguration(this IConfiguration configuration)
	{
		VaultConfiguration result = new()
		{
			Address = configuration.GetValue<string>("Address")!,
			Token = configuration.GetValue<string>("Token")!,
			MountPoint = configuration.GetValue<string>("MountPoint")!,
			Keys = []
		};

		if (configuration.GetValue<string>("Keys") is {} multiKeysStrings)
		{
			result.Keys = multiKeysStrings.Split(new[]
			{
				',',
				';'
			}, StringSplitOptions.RemoveEmptyEntries);
		}
		else
		{
			result.Keys = configuration.GetSection("Keys").Get<string[]>()!;
		}

		return result;
	}

	public static IConfigurationBuilder AddVault(this IConfigurationBuilder builder, string configurationSectionName = "Vault")
	{
		IConfigurationRoot configuration = builder.Build();
		if (configuration.GetSection(configurationSectionName) is { } section && section.Exists())
		{
			builder.AddVault(section.LoadVaultConfiguration());
		}

		return builder;
	}
}