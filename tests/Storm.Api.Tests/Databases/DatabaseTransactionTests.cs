using System.Data;
using System.Reflection;
using Storm.Api.Databases.Connections;
using Storm.Api.Databases.Services;

namespace Storm.Api.Tests.Databases;

public class DatabaseTransactionTests
{
	private sealed class FakeConnection : IDbConnection
	{
		public string ConnectionString { get; set; } = "";
		public int ConnectionTimeout => 0;
		public string Database => "";
		public ConnectionState State => ConnectionState.Open;

		public IDbTransaction BeginTransaction() => new FakeTransaction(this);
		public IDbTransaction BeginTransaction(IsolationLevel il) => new FakeTransaction(this);
		public void ChangeDatabase(string databaseName) { }
		public void Close() { }
		public IDbCommand CreateCommand() => null!;
		public void Open() { }
		public void Dispose() { }
	}

	private sealed class FakeTransaction : IDbTransaction
	{
		private readonly IDbConnection _conn;
		public int CommitCount;
		public int RollbackCount;
		public bool Disposed;
		public bool ShouldThrowZombieCheck;

		public FakeTransaction(IDbConnection conn) => _conn = conn;

		public IDbConnection? Connection => _conn;
		public IsolationLevel IsolationLevel => IsolationLevel.ReadCommitted;

		public void Commit()
		{
			CommitCount++;
			if (ShouldThrowZombieCheck)
			{
				throw NewZombieCheckException();
			}
		}

		public void Rollback() => RollbackCount++;
		public void Dispose() => Disposed = true;

		private static InvalidOperationException NewZombieCheckException()
		{
			MethodInfo? m = typeof(FakeTransaction).GetMethod(nameof(ZombieCheck), BindingFlags.Instance | BindingFlags.NonPublic);
			return (InvalidOperationException)Activator.CreateInstance(typeof(InvalidOperationException), "zombie")!
				?? throw new InvalidOperationException();
		}

		internal void ZombieCheck() { }
	}

	private static (DatabaseService svc, FakeConnection conn, FakeTransaction tx, object dbTx) BuildTransaction()
	{
		DatabaseService svc = (DatabaseService)Activator.CreateInstance(typeof(DatabaseService), [new DummyFactory()])!;
		FakeConnection conn = new();
		FakeTransaction tx = new(conn);
		Type dtType = typeof(IDatabaseTransaction).Assembly.GetType("Storm.Api.Databases.Connections.DatabaseTransaction")!;
		object dbTx = Activator.CreateInstance(dtType, [conn, tx, svc])!;

		FieldInfo txField = typeof(DatabaseService).GetField("_transaction", BindingFlags.NonPublic | BindingFlags.Instance)!;
		txField.SetValue(svc, dbTx);

		return (svc, conn, tx, dbTx);
	}

	private sealed class DummyFactory : IDatabaseConnectionFactory
	{
		public IDbConnection Create() => throw new NotSupportedException();
		public Task<IDbConnection> OpenWrite(CancellationToken ct) => throw new NotSupportedException();
		public Task<IDbConnection> OpenRead(CancellationToken ct) => throw new NotSupportedException();
	}

	[Fact]
	public void Commit_CallsUnderlyingCommit_MarksFinalized()
	{
		(DatabaseService _, FakeConnection _, FakeTransaction tx, object dbTx) = BuildTransaction();
		((IDatabaseTransaction)dbTx).Commit();
		tx.CommitCount.Should().Be(1);
	}

	[Fact]
	public void Rollback_CallsUnderlyingRollback_MarksFinalized()
	{
		(DatabaseService _, FakeConnection _, FakeTransaction tx, object dbTx) = BuildTransaction();
		((IDatabaseTransaction)dbTx).Rollback();
		tx.RollbackCount.Should().Be(1);
	}

	[Fact]
	public void Dispose_WhenNotFinalized_EndsTransaction()
	{
		(DatabaseService _, FakeConnection _, FakeTransaction tx, object dbTx) = BuildTransaction();
		((IDatabaseTransaction)dbTx).Dispose();
		tx.CommitCount.Should().Be(1);
		tx.Disposed.Should().BeTrue();
	}

	[Fact]
	public void Dispose_WhenFinalized_IsNoOp()
	{
		(DatabaseService _, FakeConnection _, FakeTransaction tx, object dbTx) = BuildTransaction();
		IDatabaseTransaction itx = (IDatabaseTransaction)dbTx;
		itx.Commit();
		itx.Dispose();
		tx.CommitCount.Should().Be(1);
		tx.Disposed.Should().BeTrue();
	}

	[Fact]
	public void Dispose_CalledTwice_IsIdempotent()
	{
		(DatabaseService _, FakeConnection _, FakeTransaction tx, object dbTx) = BuildTransaction();
		IDatabaseTransaction itx = (IDatabaseTransaction)dbTx;
		itx.Dispose();
		itx.Dispose();
		tx.Disposed.Should().BeTrue();
	}
}
