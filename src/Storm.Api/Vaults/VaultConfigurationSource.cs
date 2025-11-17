using Microsoft.Extensions.Configuration;

namespace Storm.Api.Vaults;

internal class VaultConfigurationSource : IConfigurationSource
{
	private readonly VaultConfiguration _config;

	public VaultConfigurationSource(VaultConfiguration config)
	{
		_config = config;
	}

	public IConfigurationProvider Build(IConfigurationBuilder builder)
	{
		return new VaultConfigurationProvider(_config);
	}
}