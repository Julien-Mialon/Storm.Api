using Microsoft.Extensions.DependencyInjection;
using Storm.Api.CQRS;
using Storm.Api.CQRS.Exceptions;

namespace Storm.Api.Tests.CQRS;

public class BaseActionTests
{
	public record Param(int Value);

	private sealed class DefaultValidationAction(IServiceProvider services) : BaseAction<Param, int>(services)
	{
		public bool ActionCalled;
		protected override Task<int> Action(Param parameter)
		{
			ActionCalled = true;
			return Task.FromResult(parameter.Value + 1);
		}
	}

	private sealed class OrderTrackingAction(IServiceProvider services) : BaseAction<Param, int>(services)
	{
		public List<string> Order { get; } = [];

		protected override bool ValidateParameter(Param parameter)
		{
			Order.Add("validate");
			return true;
		}

		protected override void PrepareParameter(Param parameter) => Order.Add("prepare");

		protected override Task<int> Action(Param parameter)
		{
			Order.Add("action");
			return Task.FromResult(99);
		}
	}

	private sealed class CustomValidationFalseAction(IServiceProvider services) : BaseAction<Param, int>(services)
	{
		public bool ActionCalled;
		public bool PrepareCalled;

		protected override bool ValidateParameter(Param parameter) => false;
		protected override void PrepareParameter(Param parameter) => PrepareCalled = true;
		protected override Task<int> Action(Param parameter)
		{
			ActionCalled = true;
			return Task.FromResult(1);
		}
	}

	private sealed class CustomValidationTrueAction(IServiceProvider services) : BaseAction<Param, int>(services)
	{
		protected override bool ValidateParameter(Param parameter) => true;
		protected override Task<int> Action(Param parameter) => Task.FromResult(7);
	}

	private static IServiceProvider EmptyServices() => new ServiceCollection().BuildServiceProvider();

	[Fact]
	public async Task Execute_WithNullParameter_ThrowsBadRequest()
	{
		DefaultValidationAction a = new(EmptyServices());
		Func<Task> act = () => a.Execute(null!);
		(await act.Should().ThrowAsync<DomainHttpCodeException>()).Which.Code.Should().Be(400);
	}

	[Fact]
	public async Task Execute_WithValidParameter_CallsValidateThenPrepareThenAction_InOrder()
	{
		OrderTrackingAction a = new(EmptyServices());
		await a.Execute(new Param(1));
		a.Order.Should().Equal("validate", "prepare", "action");
	}

	[Fact]
	public async Task Execute_WithCustomValidationFalse_ThrowsBadRequest()
	{
		CustomValidationFalseAction a = new(EmptyServices());
		Func<Task> act = () => a.Execute(new Param(1));
		await act.Should().ThrowAsync<DomainHttpCodeException>();
	}

	[Fact]
	public async Task Execute_WithCustomValidationTrue_ExecutesAction()
	{
		CustomValidationTrueAction a = new(EmptyServices());
		(await a.Execute(new Param(1))).Should().Be(7);
	}

	[Fact]
	public async Task Execute_ReturnsActionResult()
	{
		DefaultValidationAction a = new(EmptyServices());
		(await a.Execute(new Param(41))).Should().Be(42);
	}

	[Fact]
	public async Task Execute_PrepareParameterCalledOnlyAfterSuccessfulValidation()
	{
		CustomValidationFalseAction a = new(EmptyServices());
		try
		{
			await a.Execute(new Param(1));
		}
		catch (DomainHttpCodeException)
		{
		}
		a.PrepareCalled.Should().BeFalse();
	}

	[Fact]
	public async Task Execute_ActionNotCalled_WhenValidationFails()
	{
		CustomValidationFalseAction a = new(EmptyServices());
		try
		{
			await a.Execute(new Param(1));
		}
		catch (DomainHttpCodeException)
		{
		}
		a.ActionCalled.Should().BeFalse();
	}
}
