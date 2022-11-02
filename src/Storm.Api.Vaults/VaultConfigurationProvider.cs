using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.Commons;

namespace Storm.Api.Vaults;

internal class VaultConfigurationProvider : ConfigurationProvider
{
	private readonly VaultConfiguration _config;
	private readonly IVaultClient _client;

	public VaultConfigurationProvider(VaultConfiguration config)
	{
		_config = config;

		VaultClientSettings vaultClientSettings = new(_config.Address, new TokenAuthMethodInfo(_config.Token));

		_client = new VaultClient(vaultClientSettings);
	}

	public override void Load()
	{
		LoadAsync().Wait();
	}

	private async Task LoadAsync()
	{
		foreach (string key in _config.Keys)
		{
			Secret<SecretData> secrets = await _client.V1.Secrets.KeyValue.V2.ReadSecretAsync(key, mountPoint: _config.MountPoint);

			IDictionary<string, object> data = secrets.Data.Data;

			foreach ((string dataKey, object value) in data)
			{
				AddData(dataKey, value);
			}
		}
	}

	private void AddData(string currentKey, object value)
	{
		if (value is JObject obj)
		{
			foreach ((string key, JToken jToken) in obj)
			{
				AddData($"{currentKey}:{key}", jToken);
			}
		}
		else
		{
			Data[currentKey] = value.ToString();
		}
	}
}