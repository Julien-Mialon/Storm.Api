using Microsoft.Extensions.Configuration;
using Storm.Api.Extensions;

namespace Storm.Api.Tests.Extensions;

public class ConfigurationExtensionsTests
{
	private static IConfiguration BuildConfig(Dictionary<string, string?> values)
		=> new ConfigurationBuilder().AddInMemoryCollection(values).Build();

	[Fact]
	public void OnSection_SectionExists_InvokesAction()
	{
		IConfiguration config = BuildConfig(new() { ["mysection:key"] = "value" });
		bool invoked = false;

		config.OnSection("mysection", _ => invoked = true);

		invoked.Should().BeTrue();
	}

	[Fact]
	public void OnSection_SectionMissing_SkipsAction()
	{
		IConfiguration config = BuildConfig(new());
		bool invoked = false;

		config.OnSection("missing", _ => invoked = true);

		invoked.Should().BeFalse();
	}

	[Fact]
	public void WithSection_SectionExists_ReturnsActionResult()
	{
		IConfiguration config = BuildConfig(new() { ["mysection:key"] = "hello" });

		string? result = config.WithSection("mysection", s => s["key"]);

		result.Should().Be("hello");
	}

	[Fact]
	public void WithSection_SectionMissing_ReturnsNull()
	{
		IConfiguration config = BuildConfig(new());

		string? result = config.WithSection<string>("missing", _ => "never");

		result.Should().BeNull();
	}
}
