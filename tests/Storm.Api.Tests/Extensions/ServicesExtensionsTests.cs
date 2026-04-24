using Microsoft.Extensions.DependencyInjection;
using Storm.Api.CQRS;
using Storm.Api.Extensions;
using Storm.Api.Services;

namespace Storm.Api.Tests.Extensions;

public class ServicesExtensionsTests
{
	private sealed class FixedDateService : IDateService
	{
		public DateTime Now { get; } = new(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);
	}

	private class Dependency { }

	private class Consumer(Dependency dep)
	{
		public Dependency Dep { get; } = dep;
	}

	[Fact]
	public void Now_DelegatesToTimeProvider()
	{
		ServiceCollection services = new();
		services.AddSingleton<IDateService, FixedDateService>();
		IServiceProvider provider = services.BuildServiceProvider();

		provider.Now().Should().Be(new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc));
	}

	[Fact]
	public void Create_UsesActivatorUtilities()
	{
		ServiceCollection services = new();
		services.AddSingleton<Dependency>();
		IServiceProvider provider = services.BuildServiceProvider();

		Consumer c = provider.Create<Consumer>();
		c.Dep.Should().NotBeNull();
	}

	[Fact]
	public async Task ExecuteWithScope_Async_CreatesAndDisposesScope()
	{
		ServiceCollection services = new();
		services.AddScoped<Dependency>();
		IServiceProvider provider = services.BuildServiceProvider();

		Dependency? captured = null;
		await provider.ExecuteWithScope(async sp =>
		{
			captured = sp.GetRequiredService<Dependency>();
			await Task.CompletedTask;
		});
		captured.Should().NotBeNull();
	}

	[Fact]
	public void ExecuteWithScope_Sync_CreatesAndDisposesScope()
	{
		ServiceCollection services = new();
		services.AddScoped<Dependency>();
		IServiceProvider provider = services.BuildServiceProvider();

		Dependency? captured = null;
		provider.ExecuteWithScope(sp => { captured = sp.GetRequiredService<Dependency>(); });
		captured.Should().NotBeNull();
	}

	[Fact]
	public async Task ExecuteWithScope_WithResult_ReturnsValue()
	{
		ServiceCollection services = new();
		services.AddScoped<Dependency>();
		IServiceProvider provider = services.BuildServiceProvider();

		int result = await provider.ExecuteWithScope(sp =>
		{
			sp.GetRequiredService<Dependency>();
			return Task.FromResult(7);
		});
		result.Should().Be(7);

		int sync = provider.ExecuteWithScope(sp => 11);
		sync.Should().Be(11);
	}

	[Fact]
	public async Task ExecuteWithScope_ExceptionPropagates()
	{
		ServiceCollection services = new();
		IServiceProvider provider = services.BuildServiceProvider();

		Func<Task> act = () => provider.ExecuteWithScope(_ => throw new InvalidOperationException("boom"));
		await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
	}

	private sealed class TestParam { public int Value; }
	private sealed class TestAction : IAction<TestParam, int>
	{
		public Task<int> Execute(TestParam parameter) => Task.FromResult(parameter.Value + 1);
	}

	[Fact]
	public async Task ExecuteAction_ResolvesActionAndExecutes()
	{
		ServiceCollection services = new();
		IServiceProvider provider = services.BuildServiceProvider();

		int result = await provider.ExecuteAction<TestAction, TestParam, int>(new TestParam { Value = 41 });
		result.Should().Be(42);
	}
}
