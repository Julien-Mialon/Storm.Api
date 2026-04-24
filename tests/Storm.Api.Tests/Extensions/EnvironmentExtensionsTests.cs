using Microsoft.Extensions.Hosting;
using Storm.Api.Extensions;

namespace Storm.Api.Tests.Extensions;

public class EnvironmentExtensionsTests
{
	private sealed class StubEnv(string name) : IHostEnvironment
	{
		public string EnvironmentName { get; set; } = name;
		public string ApplicationName { get; set; } = "";
		public string ContentRootPath { get; set; } = "";
		public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
	}

	[Fact]
	public void SimpleEnvironmentName_NoDelimiter_ReturnsOriginal()
	{
		new StubEnv("Production").SimpleEnvironmentName().Should().Be("Production");
	}

	[Fact]
	public void SimpleEnvironmentName_WithDashSuffix_ReturnsPrefix()
	{
		new StubEnv("Production-canary").SimpleEnvironmentName().Should().Be("Production");
	}

	[Fact]
	public void SimpleEnvironmentName_LeadingDelimiter_ReturnsOriginal()
	{
		new StubEnv("-Production").SimpleEnvironmentName().Should().Be("-Production");
	}
}
