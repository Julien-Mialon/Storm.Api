using Storm.Api.Extensions;

namespace Storm.Api.Tests.Extensions;

public class TasksExtensionsTests
{
	[Fact]
	public async Task AsTask_ReturnsCompletedTaskWithValue()
	{
		Task<int> t = 42.AsTask();
		t.IsCompletedSuccessfully.Should().BeTrue();
		(await t).Should().Be(42);
	}

	[Fact]
	public async Task AsTaskNullable_Struct_WrapsValue()
	{
		Task<int?> t = 42.AsTaskNullable();
		(await t).Should().Be(42);
	}

	[Fact]
	public async Task AsTaskNullable_Null_ReturnsTaskWithNull()
	{
		int? value = null;
		Task<int?> t = value.AsTaskNullable();
		(await t).Should().BeNull();
	}

	[Fact]
	public async Task WaitForCancellation_AlreadyCancelled_CompletesImmediately()
	{
		CancellationTokenSource cts = new();
		cts.Cancel();
		await cts.Token.WaitForCancellation();
	}

	[Fact]
	public async Task WaitForCancellation_CompletesWhenTokenCancelled()
	{
		CancellationTokenSource cts = new();
		Task t = cts.Token.WaitForCancellation();
		t.IsCompleted.Should().BeFalse();
		cts.Cancel();
		await t;
		t.IsCompleted.Should().BeTrue();
	}
}
