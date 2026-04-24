using Microsoft.Extensions.Logging;
using Storm.Api.Logs;
using Storm.Api.Logs.Interfaces;

namespace Storm.Api.Tests.Logs;

public class LogServiceTests
{
	private sealed class RecordingSink : ILogSink
	{
		public List<(LogLevel Level, string Entry)> Entries { get; } = [];
		public void Enqueue(LogLevel level, string entry) => Entries.Add((level, entry));
	}

	private sealed class CountingAppender(bool multipleAllowed = false) : ILogAppender
	{
		public int Calls { get; private set; }
		public bool MultipleAllowed { get; } = multipleAllowed;
		public void Append(IObjectWriter logEntry)
		{
			Calls++;
			logEntry.WriteProperty("appender", "yes");
		}
	}

	private sealed class MultiAllowedAppender : ILogAppender
	{
		public bool MultipleAllowed => true;
		public void Append(IObjectWriter logEntry) => logEntry.WriteProperty("multi", "y");
	}

	[Fact]
	public void Log_BelowMinimumLevel_Discarded()
	{
		RecordingSink sink = new();
		LogService svc = new(_ => sink, LogLevel.Warning);
		svc.Log(LogLevel.Information, w => w.WriteProperty("msg", "hello"));
		sink.Entries.Should().BeEmpty();
	}

	[Fact]
	public void Log_AtOrAboveMinimumLevel_Forwarded()
	{
		RecordingSink sink = new();
		LogService svc = new(_ => sink, LogLevel.Information);
		svc.Log(LogLevel.Information, w => w.WriteProperty("msg", "hello"));
		svc.Log(LogLevel.Error, w => w.WriteProperty("msg", "boom"));
		sink.Entries.Should().HaveCount(2);
	}

	[Fact]
	public void Log_AppliesAllAppenders()
	{
		RecordingSink sink = new();
		LogService svc = new(_ => sink, LogLevel.Trace);
		CountingAppender a1 = new();
		CountingAppender a2 = new(multipleAllowed: true);
		svc.WithAppender(a1).WithAppender(a2);

		svc.Log(LogLevel.Information, w => w.WriteProperty("msg", "x"));

		a1.Calls.Should().Be(1);
		a2.Calls.Should().Be(1);
	}

	[Fact]
	public void WithAppender_DuplicateWhenMultipleAllowedFalse_DeduplicatesByType()
	{
		RecordingSink sink = new();
		LogService svc = new(_ => sink, LogLevel.Trace);
		CountingAppender a1 = new();
		CountingAppender a2 = new();
		svc.WithAppender(a1).WithAppender(a2);

		svc.Log(LogLevel.Information, w => w.WriteProperty("msg", "x"));
		a1.Calls.Should().Be(1);
		a2.Calls.Should().Be(0);
	}

	[Fact]
	public void WithAppender_DuplicateWhenMultipleAllowedTrue_AddsBoth()
	{
		RecordingSink sink = new();
		LogService svc = new(_ => sink, LogLevel.Trace);
		MultiAllowedAppender m1 = new();
		MultiAllowedAppender m2 = new();
		svc.WithAppender(m1).WithAppender(m2);
		svc.Log(LogLevel.Information, w => w.WriteProperty("msg", "x"));
		sink.Entries.Should().ContainSingle();
	}

	[Fact]
	public void WithAppenders_BatchRegistration_AddsAll()
	{
		RecordingSink sink = new();
		LogService svc = new(_ => sink, LogLevel.Trace);
		CountingAppender a1 = new();
		MultiAllowedAppender m1 = new();
		svc.WithAppenders(a1, m1);
		svc.Log(LogLevel.Information, w => w.WriteProperty("msg", "x"));
		a1.Calls.Should().Be(1);
	}

	[Fact]
	public void Log_NullContent_HandledWithoutThrowing()
	{
		RecordingSink sink = new();
		LogService svc = new(_ => sink, LogLevel.Trace);
		Action act = () => svc.Log(LogLevel.Information, _ => { });
		act.Should().NotThrow();
	}
}
