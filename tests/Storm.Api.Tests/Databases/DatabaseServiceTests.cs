using System.Data;
using Storm.Api.Databases.Connections;
using Storm.Api.Databases.Services;

namespace Storm.Api.Tests.Databases;

public class DatabaseServiceTests
{
	private sealed class FakeConnection : IDbConnection
	{
		public string ConnectionString { get; set; } = "";
		public int ConnectionTimeout => 0;
		public string Database => "";
		public ConnectionState State => ConnectionState.Open;

		public IDbTransaction BeginTransaction() => new FakeTransaction();
		public IDbTransaction BeginTransaction(IsolationLevel il) => new FakeTransaction();
		public void ChangeDatabase(string databaseName) { }
		public void Close() { }
		public IDbCommand CreateCommand() => null!;
		public void Open() { }
		public void Dispose() { }
	}

	private sealed class FakeTransaction : IDbTransaction
	{
		public IDbConnection? Connection => null;
		public IsolationLevel IsolationLevel => IsolationLevel.ReadCommitted;
		public void Commit() { }
		public void Rollback() { }
		public void Dispose() { }
	}

	private sealed class CountingFactory : IDatabaseConnectionFactory
	{
		public int OpenWriteCalls;
		public int OpenReadCalls;
		public bool SupportsReadReplicas { get; set; }

		public IDbConnection Create() => new FakeConnection();
		public Task<IDbConnection> OpenWrite(CancellationToken ct)
		{
			OpenWriteCalls++;
			return Task.FromResult<IDbConnection>(new FakeConnection());
		}
		public Task<IDbConnection> OpenRead(CancellationToken ct)
		{
			OpenReadCalls++;
			return Task.FromResult<IDbConnection>(new FakeConnection());
		}
	}

	[Fact]
	public async Task GetWriteConnection_FirstCall_CreatesConnection()
	{
		CountingFactory f = new();
		DatabaseService svc = new(f);
		await svc.GetWriteConnection();
		f.OpenWriteCalls.Should().Be(1);
	}

	[Fact]
	public async Task GetWriteConnection_SecondCall_ReusesConnection()
	{
		CountingFactory f = new();
		DatabaseService svc = new(f);
		IDbConnection a = await svc.GetWriteConnection();
		IDbConnection b = await svc.GetWriteConnection();
		f.OpenWriteCalls.Should().Be(1);
		a.Should().BeSameAs(b);
	}

	[Fact]
	public async Task GetReadConnection_NoReadReplica_RoutesToWrite()
	{
		CountingFactory f = new() { SupportsReadReplicas = false };
		DatabaseService svc = new(f);
		await svc.GetReadConnection();
		f.OpenReadCalls.Should().Be(0);
		f.OpenWriteCalls.Should().Be(1);
	}

	[Fact]
	public async Task GetReadConnection_WithReadReplica_UsesReadConnection()
	{
		CountingFactory f = new() { SupportsReadReplicas = true };
		DatabaseService svc = new(f);
		await svc.GetReadConnection();
		f.OpenReadCalls.Should().Be(1);
	}

	[Fact]
	public async Task CreateTransaction_NotInTransaction_ReturnsRealTransaction()
	{
		CountingFactory f = new();
		DatabaseService svc = new(f);
		using IDatabaseTransaction tx = await svc.CreateTransaction();
		tx.GetType().Name.Should().Be("DatabaseTransaction");
	}

	[Fact]
	public async Task CreateTransaction_NestedCall_ReturnsDummyTransaction()
	{
		CountingFactory f = new();
		DatabaseService svc = new(f);
		using IDatabaseTransaction first = await svc.CreateTransaction();
		using IDatabaseTransaction nested = await svc.CreateTransaction();
		nested.GetType().Name.Should().Be("DummyTransaction");
	}

	[Fact]
	public async Task InTransaction_Success_CommitsAutomatically()
	{
		CountingFactory f = new();
		DatabaseService svc = new(f);
		bool ran = false;
		await svc.InTransaction(_ => { ran = true; return Task.CompletedTask; });
		ran.Should().BeTrue();
	}

	[Fact]
	public async Task InTransaction_Throws_RollsBackAndRethrows()
	{
		CountingFactory f = new();
		DatabaseService svc = new(f);
		Func<Task> act = () => svc.InTransaction(_ => throw new InvalidOperationException("boom"));
		await act.Should().ThrowAsync<InvalidOperationException>();
	}
}
