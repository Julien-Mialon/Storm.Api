using Microsoft.Extensions.DependencyInjection;
using Storm.Api.CQRS;
using Storm.Api.CQRS.Exceptions;

namespace Storm.Api.Tests.CQRS;

public class BaseAuthenticatedActionTests
{
	public record Param(int Value);
	public record Account(string Name);

	private sealed class StubAuthenticator(Account? account) : IActionAuthenticator<Account>
	{
		public int CallCount { get; private set; }
		public Task<Account?> Authenticate()
		{
			CallCount++;
			return Task.FromResult(account);
		}
	}

	private sealed class DefaultAuthorizeAction(IServiceProvider services) : BaseAuthenticatedAction<Param, string, Account>(services)
	{
		public Account? CapturedAccount;
		public Param? CapturedParameter;

		protected override Task<string> Action(Param parameter, Account account)
		{
			CapturedAccount = account;
			CapturedParameter = parameter;
			return Task.FromResult($"{parameter.Value}:{account.Name}");
		}
	}

	private sealed class OrderTrackingAction(IServiceProvider services) : BaseAuthenticatedAction<Param, string, Account>(services)
	{
		public List<string> Order { get; } = [];

		protected override bool ValidateParameter(Param parameter)
		{
			Order.Add("validate");
			return true;
		}
		protected override void PrepareParameter(Param parameter) => Order.Add("prepare");
		protected override Task Authorize(Param parameter, Account account)
		{
			Order.Add("authorize");
			return Task.CompletedTask;
		}
		protected override Task<string> Action(Param parameter, Account account)
		{
			Order.Add("action");
			return Task.FromResult("ok");
		}
	}

	private sealed class AuthorizeThrowsAction(IServiceProvider services) : BaseAuthenticatedAction<Param, string, Account>(services)
	{
		public bool ActionCalled;
		protected override Task Authorize(Param parameter, Account account)
			=> throw new DomainHttpCodeException(System.Net.HttpStatusCode.Forbidden);

		protected override Task<string> Action(Param parameter, Account account)
		{
			ActionCalled = true;
			return Task.FromResult("");
		}
	}

	private static IServiceProvider BuildServices(Account? account)
	{
		ServiceCollection sc = new();
		sc.AddSingleton<IActionAuthenticator<Account>>(new StubAuthenticator(account));
		return sc.BuildServiceProvider();
	}

	[Fact]
	public async Task Execute_WithNullParameter_ThrowsBadRequest()
	{
		DefaultAuthorizeAction a = new(BuildServices(new Account("bob")));
		Func<Task> act = () => a.Execute(null!);
		(await act.Should().ThrowAsync<DomainHttpCodeException>()).Which.Code.Should().Be(400);
	}

	[Fact]
	public async Task Execute_WhenAuthenticatorReturnsNull_ThrowsUnauthorized()
	{
		DefaultAuthorizeAction a = new(BuildServices(null));
		Func<Task> act = () => a.Execute(new Param(1));
		(await act.Should().ThrowAsync<DomainHttpCodeException>()).Which.Code.Should().Be(401);
	}

	[Fact]
	public async Task Execute_CallsValidateThenPrepareThenAuthenticateThenAuthorizeThenAction_InOrder()
	{
		OrderTrackingAction a = new(BuildServices(new Account("bob")));
		await a.Execute(new Param(1));
		a.Order.Should().Equal("validate", "prepare", "authorize", "action");
	}

	[Fact]
	public async Task Execute_AuthorizeCalledWithResolvedAccount()
	{
		DefaultAuthorizeAction a = new(BuildServices(new Account("alice")));
		await a.Execute(new Param(1));
		a.CapturedAccount!.Name.Should().Be("alice");
	}

	[Fact]
	public async Task Execute_ActionCalledWithBothParameterAndAccount()
	{
		DefaultAuthorizeAction a = new(BuildServices(new Account("alice")));
		string result = await a.Execute(new Param(42));
		result.Should().Be("42:alice");
		a.CapturedParameter!.Value.Should().Be(42);
	}

	[Fact]
	public async Task Execute_DefaultAuthorizeIsNoOp()
	{
		DefaultAuthorizeAction a = new(BuildServices(new Account("bob")));
		await a.Execute(new Param(1));
	}

	[Fact]
	public async Task Execute_CustomAuthorizeCanThrow_PreventsActionFromRunning()
	{
		AuthorizeThrowsAction a = new(BuildServices(new Account("bob")));
		Func<Task> act = () => a.Execute(new Param(1));
		await act.Should().ThrowAsync<DomainHttpCodeException>();
		a.ActionCalled.Should().BeFalse();
	}

	[Fact]
	public async Task Execute_ReturnsActionResult_OnHappyPath()
	{
		DefaultAuthorizeAction a = new(BuildServices(new Account("alice")));
		(await a.Execute(new Param(7))).Should().Be("7:alice");
	}
}
