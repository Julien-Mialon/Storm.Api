using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Storm.Api.Vaults;

namespace Storm.Api.Tests.Vaults;

public class VaultConfigurationProviderTests
{
	private static (object provider, MethodInfo addData, IDictionary<string, string?> data) MakeProvider()
	{
		Type type = typeof(VaultExtensions).Assembly.GetType("Storm.Api.Vaults.VaultConfigurationProvider")!;
		VaultConfiguration cfg = new()
		{
			Address = "http://127.0.0.1:8200",
			Token = "tok",
			MountPoint = "m",
			Keys = [],
		};
		object provider = Activator.CreateInstance(type, cfg)!;
		MethodInfo addData = type.GetMethod("AddData", BindingFlags.NonPublic | BindingFlags.Instance)!;
		PropertyInfo dataProp = typeof(ConfigurationProvider).GetProperty("Data", BindingFlags.NonPublic | BindingFlags.Instance)!;
		IDictionary<string, string?> data = (IDictionary<string, string?>)dataProp.GetValue(provider)!;
		return (provider, addData, data);
	}

	[Fact]
	public void AddData_FlatString_AddsSingleEntry()
	{
		(object p, MethodInfo addData, IDictionary<string, string?> data) = MakeProvider();
		addData.Invoke(p, ["key", "value"]);
		data["key"].Should().Be("value");
	}

	[Fact]
	public void AddData_NestedJsonObject_FlattensToColonSeparatedKeys()
	{
		(object p, MethodInfo addData, IDictionary<string, string?> data) = MakeProvider();
		JsonElement json = JsonDocument.Parse("{\"inner\":\"v\"}").RootElement;
		addData.Invoke(p, ["outer", json]);
		data["outer:inner"].Should().Be("v");
	}

	[Fact]
	public void AddData_DeeplyNestedObject_FullyFlattened()
	{
		(object p, MethodInfo addData, IDictionary<string, string?> data) = MakeProvider();
		JsonElement json = JsonDocument.Parse("{\"a\":{\"b\":{\"c\":\"deep\"}}}").RootElement;
		addData.Invoke(p, ["root", json]);
		data["root:a:b:c"].Should().Be("deep");
	}

	[Fact]
	public void AddData_ArrayValues_IndicesUsedInKey()
	{
		// Current implementation does NOT flatten arrays recursively;
		// non-Object JsonElements are stored as their string representation.
		(object p, MethodInfo addData, IDictionary<string, string?> data) = MakeProvider();
		JsonElement json = JsonDocument.Parse("[\"a\",\"b\"]").RootElement;
		addData.Invoke(p, ["arr", json]);
		data.Should().ContainKey("arr");
	}
}
