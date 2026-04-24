using Microsoft.Extensions.Configuration;
using Storm.Api.Vaults;

namespace Storm.Api.Tests.Vaults;

public class VaultExtensionsTests
{
	private static IConfigurationRoot BuildConfig(Dictionary<string, string?> values)
		=> new ConfigurationBuilder().AddInMemoryCollection(values).Build();

	[Fact]
	public void LoadVaultConfiguration_SectionPresent_PopulatesFields()
	{
		IConfiguration config = BuildConfig(new()
		{
			["Address"] = "http://vault:8200",
			["Token"] = "tok",
			["MountPoint"] = "secret",
			["Keys"] = "a,b,c",
		});

		VaultConfiguration vc = config.LoadVaultConfiguration();

		vc.Address.Should().Be("http://vault:8200");
		vc.Token.Should().Be("tok");
		vc.MountPoint.Should().Be("secret");
		vc.Keys.Should().Equal("a", "b", "c");
	}

	[Fact]
	public void LoadVaultConfiguration_KeysAsCommaSeparatedString_ParsedToArray()
	{
		IConfiguration config = BuildConfig(new()
		{
			["Address"] = "a",
			["Token"] = "t",
			["MountPoint"] = "m",
			["Keys"] = "one,two",
		});
		config.LoadVaultConfiguration().Keys.Should().Equal("one", "two");
	}

	[Fact]
	public void LoadVaultConfiguration_KeysAsSemicolonSeparatedString_ParsedToArray()
	{
		IConfiguration config = BuildConfig(new()
		{
			["Address"] = "a",
			["Token"] = "t",
			["MountPoint"] = "m",
			["Keys"] = "one;two",
		});
		config.LoadVaultConfiguration().Keys.Should().Equal("one", "two");
	}

	[Fact]
	public void LoadVaultConfiguration_KeysAsArray_ParsedDirectly()
	{
		IConfiguration config = BuildConfig(new()
		{
			["Address"] = "a",
			["Token"] = "t",
			["MountPoint"] = "m",
			["Keys:0"] = "one",
			["Keys:1"] = "two",
		});
		config.LoadVaultConfiguration().Keys.Should().Equal("one", "two");
	}

	[Fact]
	public void AddVault_SectionMissing_NoProviderAdded()
	{
		ConfigurationBuilder builder = new();
		builder.AddInMemoryCollection(new Dictionary<string, string?> { ["Other"] = "x" });
		int before = builder.Sources.Count;
		builder.AddVault();
		int after = builder.Sources.Count;

		after.Should().Be(before);
	}

	[Fact]
	public void AddVault_ConfigurationObject_RegistersProvider()
	{
		ConfigurationBuilder builder = new();
		VaultConfiguration vc = new()
		{
			Address = "http://vault:8200",
			Token = "tok",
			MountPoint = "secret",
			Keys = ["a"],
		};
		builder.AddVault(vc);
		builder.Sources.Should().ContainSingle();
		builder.Sources[0].GetType().Name.Should().Be("VaultConfigurationSource");
	}
}
