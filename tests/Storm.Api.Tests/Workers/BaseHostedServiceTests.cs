using Microsoft.Extensions.DependencyInjection;
using Storm.Api.Workers.HostedServices;

namespace Storm.Api.Tests.Workers;

public class BaseHostedServiceTests
{
	private sealed class TestService(IServiceProvider services) : BaseHostedService(services)
	{
		public T Call<T>() where T : class => Resolve<T>();
		protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;
	}

	private sealed class MyDep { }

	[Fact]
	public void Resolve_ServiceRegistered_ReturnsInstance()
	{
		ServiceCollection sc = new();
		sc.AddSingleton<MyDep>();
		IServiceProvider sp = sc.BuildServiceProvider();
		TestService hs = new(sp);
		hs.Call<MyDep>().Should().NotBeNull();
	}

	[Fact]
	public void Resolve_ServiceMissing_ThrowsInvalidOperation()
	{
		ServiceCollection sc = new();
		IServiceProvider sp = sc.BuildServiceProvider();
		TestService hs = new(sp);
		Action act = () => hs.Call<MyDep>();
		act.Should().Throw<InvalidOperationException>();
	}
}
