using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Storm.Api.Authentications.Jwts;
using Storm.Api.Authentications.Refresh;
using Storm.Api.Authentications.Refresh.Storage;
using Storm.Api.Authentications.Refresh.Transport;
using Storm.Api.CQRS.Exceptions;
using Storm.Api.Databases.Connections;
using Storm.Api.Databases.Models;
using Storm.Api.Databases.Repositories;
using Storm.Api.Databases.Services;
using Storm.Api.Dtos;

namespace Storm.Api.Tests.Authentications;

public class BaseRefreshActionTests
{
	private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
	{
		public override DateTimeOffset GetUtcNow() => now;
	}

	public sealed class Account : IGuidEntity
	{
		public Guid Id { get; set; }
	}

	public sealed class Parameter : IRefreshTokenParameterConvertible
	{
		public string RefreshToken { get; set; } = "";
		public RefreshTokenParameter AsRefreshTokenParameter() => new() { RefreshToken = RefreshToken };
	}

	private sealed class TestRefreshAction(IServiceProvider services, Func<Account, Task>? validateAccount = null)
		: BaseRefreshAction<Parameter, Account>(services)
	{
		public int ValidateAccountCalls;

		protected override async Task ValidateAccount(Account account)
		{
			ValidateAccountCalls++;
			if (validateAccount is not null)
			{
				await validateAccount(account);
			}
		}
	}

	private sealed class StubTransport : IRefreshTokenTransport
	{
		public string? TokenToReturn = "inbound-refresh-token";
		public bool ValidateResult = true;
		public int ClearTokenCalls;
		public int EmitTokenCalls;
		public string? EmittedToken;
		public TimeSpan EmittedDuration;

		public string? ReadToken(RefreshTokenParameter parameter) => TokenToReturn;
		public bool ValidateTransport(string refreshToken) => ValidateResult;
		public void EmitToken(string refreshToken, TimeSpan duration, LoginResponse response)
		{
			EmitTokenCalls++;
			EmittedToken = refreshToken;
			EmittedDuration = duration;
		}
		public void ClearToken() => ClearTokenCalls++;
	}

	private sealed class StubTransportResolver(IRefreshTokenTransport transport) : IRefreshTokenTransportResolver
	{
		public IRefreshTokenTransport Resolve() => transport;
	}

	private sealed class StubStorage : IRefreshTokenStorage
	{
		public bool ValidateResult = true;
		public bool RotateResult = true;
		public int ValidateCalls;
		public int RotateCalls;
		public string? ValidatedJti;
		public string? OldJti;
		public string? NewJti;

		public Task StoreAsync(Guid accountId, string jti, DateTime expiresAt) => Task.CompletedTask;
		public Task<bool> ValidateAsync(string jti)
		{
			ValidateCalls++;
			ValidatedJti = jti;
			return Task.FromResult(ValidateResult);
		}
		public Task<bool> RotateAsync(string oldJti, Guid accountId, string newJti, DateTime newExpiresAt)
		{
			RotateCalls++;
			OldJti = oldJti;
			NewJti = newJti;
			return Task.FromResult(RotateResult);
		}
		public Task RevokeAsync(string jti) => Task.CompletedTask;
		public Task RevokeAllForAccountAsync(Guid accountId) => Task.CompletedTask;
	}

	private sealed class StubRepository(Account? account) : IGuidRepository<Account>
	{
		public int GetByIdCalls;

		public Task<Account?> GetById(Guid id)
		{
			GetByIdCalls++;
			return Task.FromResult(account);
		}

		public Task<bool> ExistsById(Guid id) => throw new NotImplementedException();
		public Task<List<Account>> List() => throw new NotImplementedException();
		public Task<Account> Create(Account entity) => throw new NotImplementedException();
		public Task Create(List<Account> entities) => throw new NotImplementedException();
		public Task<Account> Update(Account entity) => throw new NotImplementedException();
		public Task Update(List<Account> entity) => throw new NotImplementedException();
		public Task<bool> Delete(Guid id) => throw new NotImplementedException();
		public Task<bool> Delete(Account entity) => throw new NotImplementedException();
	}

	private sealed class StubDatabaseService : IDatabaseService
	{
		public Task<IDbConnection> Connection => Task.FromResult<IDbConnection>(null!);
		public Task<IDbConnection> GetConnection(CancellationToken ct = default) => Task.FromResult<IDbConnection>(null!);
		public Task<IDbConnection> GetWriteConnection(CancellationToken ct = default) => Task.FromResult<IDbConnection>(null!);
		public Task<IDbConnection> GetReadConnection(CancellationToken ct = default) => Task.FromResult<IDbConnection>(null!);
		public Task<IDatabaseTransaction> CreateTransaction(IsolationLevel? isolationLevel = null, CancellationToken ct = default) => throw new NotImplementedException();

		public async Task InTransaction(Func<IDatabaseTransaction, Task> action, IsolationLevel? isolationLevel = null, CancellationToken ct = default)
		{
			await action(null!);
		}

		public async Task<T> InTransaction<T>(Func<IDatabaseTransaction, Task<T>> action, IsolationLevel? isolationLevel = null, CancellationToken ct = default)
		{
			return await action(null!);
		}

		public void Dispose() { }
	}

	private const string AccessAud = "access-aud";
	private const string AccessIss = "access-iss";
	private const string RefreshAud = "refresh-aud";
	private const string RefreshIss = "refresh-iss";
	private static readonly byte[] AccessKey = System.Text.Encoding.UTF8.GetBytes("0123456789012345678901234567890123456789");
	private static readonly byte[] RefreshKey = System.Text.Encoding.UTF8.GetBytes("abcdefghijklmnopqrstuvwxyz01234567890123");

	private sealed class Harness
	{
		public StubTransport Transport { get; } = new();
		public StubStorage Storage { get; } = new();
		public StubRepository Repository { get; set; } = new(new Account { Id = Guid.NewGuid() });
		public JwtService<RefreshTokenMarker> RefreshService { get; }
		public JwtService<Account> AccessService { get; }
		public IJwtTokenService TokenService { get; }
		public IServiceProvider Services { get; private set; } = null!;

		public Harness()
		{
			Type tokenServiceType = typeof(IJwtTokenService).Assembly.GetType("Storm.Api.Authentications.Jwts.JwtTokenService")!;
			TokenService = (IJwtTokenService)Activator.CreateInstance(tokenServiceType, TimeProvider.System)!;

			RefreshService = new JwtService<RefreshTokenMarker>(TokenService, new JwtConfiguration<RefreshTokenMarker>
			{
				Key = RefreshKey,
				Audience = RefreshAud,
				Issuer = RefreshIss,
				Duration = TimeSpan.FromDays(7),
			});
			AccessService = new JwtService<Account>(TokenService, new JwtConfiguration<Account>
			{
				Key = AccessKey,
				Audience = AccessAud,
				Issuer = AccessIss,
				Duration = TimeSpan.FromMinutes(15),
			});
		}

		public Harness Build()
		{
			ServiceCollection sc = new();
			sc.AddSingleton<IRefreshTokenStorage>(Storage);
			sc.AddSingleton<IRefreshTokenTransportResolver>(new StubTransportResolver(Transport));
			sc.AddSingleton(RefreshService);
			sc.AddSingleton(AccessService);
			sc.AddSingleton<IGuidRepository<Account>>(_ => Repository);
			sc.AddSingleton<IDatabaseService>(new StubDatabaseService());
			sc.AddSingleton<TimeProvider>(new FixedTimeProvider(DateTimeOffset.UtcNow));
			Services = sc.BuildServiceProvider();
			return this;
		}

		public string IssueRefreshToken(Guid accountId, string jti)
		{
			return TokenService.GenerateToken(accountId, RefreshAud, RefreshIss, TimeSpan.FromDays(7), RefreshKey,
				new Dictionary<string, string> { ["jti"] = jti });
		}
	}

	[Fact]
	public async Task Action_ReadsTokenFromTransport()
	{
		Harness h = new();
		Guid accountId = Guid.NewGuid();
		h.Repository = new StubRepository(new Account { Id = accountId });
		string jti = Guid.NewGuid().ToString("N");
		h.Transport.TokenToReturn = h.IssueRefreshToken(accountId, jti);
		h.Build();

		TestRefreshAction action = new(h.Services);
		await action.Execute(new Parameter());

		h.Storage.ValidatedJti.Should().Be(jti);
	}

	[Fact]
	public async Task Action_TransportValidationFails_ThrowsUnauthorized()
	{
		Harness h = new();
		h.Transport.ValidateResult = false;
		h.Build();

		TestRefreshAction action = new(h.Services);
		Func<Task> act = () => action.Execute(new Parameter());

		(await act.Should().ThrowAsync<DomainHttpCodeException>()).Which.Code.Should().Be(401);
		h.Storage.RotateCalls.Should().Be(0);
	}

	[Fact]
	public async Task Action_InvalidJwt_ThrowsUnauthorized()
	{
		Harness h = new();
		h.Transport.TokenToReturn = "not.a.valid.jwt";
		h.Build();

		TestRefreshAction action = new(h.Services);
		Func<Task> act = () => action.Execute(new Parameter());

		(await act.Should().ThrowAsync<DomainHttpCodeException>()).Which.Code.Should().Be(401);
		h.Transport.ClearTokenCalls.Should().Be(1);
		h.Storage.RotateCalls.Should().Be(0);
	}

	[Fact]
	public async Task Action_MissingJti_ThrowsUnauthorized()
	{
		Harness h = new();
		Guid accountId = Guid.NewGuid();
		// token without jti claim
		h.Transport.TokenToReturn = h.TokenService.GenerateToken(accountId, RefreshAud, RefreshIss, TimeSpan.FromDays(7), RefreshKey);
		h.Build();

		TestRefreshAction action = new(h.Services);
		Func<Task> act = () => action.Execute(new Parameter());

		(await act.Should().ThrowAsync<DomainHttpCodeException>()).Which.Code.Should().Be(401);
		h.Storage.ValidateCalls.Should().Be(0);
		h.Storage.RotateCalls.Should().Be(0);
	}

	[Fact]
	public async Task Action_JtiNotInStorage_ThrowsUnauthorized()
	{
		Harness h = new();
		h.Storage.ValidateResult = false;
		Guid accountId = Guid.NewGuid();
		string jti = Guid.NewGuid().ToString("N");
		h.Transport.TokenToReturn = h.IssueRefreshToken(accountId, jti);
		h.Build();

		TestRefreshAction action = new(h.Services);
		Func<Task> act = () => action.Execute(new Parameter());

		(await act.Should().ThrowAsync<DomainHttpCodeException>()).Which.Code.Should().Be(401);
		h.Storage.RotateCalls.Should().Be(0);
	}

	[Fact]
	public async Task Action_AccountMissing_ThrowsUnauthorizedAndClearsCookie()
	{
		Harness h = new();
		h.Repository = new StubRepository(null);
		Guid accountId = Guid.NewGuid();
		string jti = Guid.NewGuid().ToString("N");
		h.Transport.TokenToReturn = h.IssueRefreshToken(accountId, jti);
		h.Build();

		TestRefreshAction action = new(h.Services);
		Func<Task> act = () => action.Execute(new Parameter());

		(await act.Should().ThrowAsync<DomainHttpCodeException>()).Which.Code.Should().Be(401);
		h.Transport.ClearTokenCalls.Should().Be(1);
		h.Storage.RotateCalls.Should().Be(0);
	}

	[Fact]
	public async Task Action_AccountValidationFails_ThrowsUnauthorized_OldTokenNotRotated()
	{
		Harness h = new();
		Guid accountId = Guid.NewGuid();
		h.Repository = new StubRepository(new Account { Id = accountId });
		string jti = Guid.NewGuid().ToString("N");
		h.Transport.TokenToReturn = h.IssueRefreshToken(accountId, jti);
		h.Build();

		TestRefreshAction action = new(h.Services, _ => throw new DomainHttpCodeException(System.Net.HttpStatusCode.Unauthorized));
		Func<Task> act = () => action.Execute(new Parameter());

		(await act.Should().ThrowAsync<DomainHttpCodeException>()).Which.Code.Should().Be(401);
		h.Storage.RotateCalls.Should().Be(0);
		h.Transport.EmitTokenCalls.Should().Be(0);
	}

	[Fact]
	public async Task Action_HappyPath_GeneratesNewAccessAndRefreshTokens()
	{
		Harness h = new();
		Guid accountId = Guid.NewGuid();
		h.Repository = new StubRepository(new Account { Id = accountId });
		string oldJti = Guid.NewGuid().ToString("N");
		h.Transport.TokenToReturn = h.IssueRefreshToken(accountId, oldJti);
		h.Build();

		TestRefreshAction action = new(h.Services);
		LoginResponse response = await action.Execute(new Parameter());

		response.AccessToken.Should().NotBeNullOrEmpty();
		h.Storage.RotateCalls.Should().Be(1);
		h.Storage.OldJti.Should().Be(oldJti);
		h.Storage.NewJti.Should().NotBeNullOrEmpty().And.NotBe(oldJti);
		h.Transport.EmitTokenCalls.Should().Be(1);
		h.Transport.EmittedToken.Should().NotBeNullOrEmpty();
		action.ValidateAccountCalls.Should().Be(1);
	}

	[Fact]
	public async Task Action_HappyPath_RotatesJtiAtomically()
	{
		Harness h = new();
		Guid accountId = Guid.NewGuid();
		h.Repository = new StubRepository(new Account { Id = accountId });
		string oldJti = Guid.NewGuid().ToString("N");
		h.Transport.TokenToReturn = h.IssueRefreshToken(accountId, oldJti);
		h.Build();

		TestRefreshAction action = new(h.Services);
		await action.Execute(new Parameter());

		h.Storage.OldJti.Should().Be(oldJti);
		h.Storage.NewJti.Should().NotBe(oldJti);
		h.Storage.NewJti.Should().MatchRegex("^[0-9a-f]{32}$");
	}

	[Fact]
	public async Task Action_RotateAsyncReturnsFalse_ThrowsUnauthorized()
	{
		Harness h = new();
		h.Storage.RotateResult = false;
		Guid accountId = Guid.NewGuid();
		h.Repository = new StubRepository(new Account { Id = accountId });
		string jti = Guid.NewGuid().ToString("N");
		h.Transport.TokenToReturn = h.IssueRefreshToken(accountId, jti);
		h.Build();

		TestRefreshAction action = new(h.Services);
		Func<Task> act = () => action.Execute(new Parameter());

		(await act.Should().ThrowAsync<DomainHttpCodeException>()).Which.Code.Should().Be(401);
		h.Transport.EmitTokenCalls.Should().Be(0);
	}

	[Fact]
	public async Task Action_MissingInboundToken_ThrowsUnauthorized()
	{
		Harness h = new();
		h.Transport.TokenToReturn = null;
		h.Build();

		TestRefreshAction action = new(h.Services);
		Func<Task> act = () => action.Execute(new Parameter());

		(await act.Should().ThrowAsync<DomainHttpCodeException>()).Which.Code.Should().Be(401);
		h.Storage.ValidateCalls.Should().Be(0);
	}
}
