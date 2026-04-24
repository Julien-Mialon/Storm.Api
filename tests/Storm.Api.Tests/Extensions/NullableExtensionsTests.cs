using Storm.Api.Extensions;

namespace Storm.Api.Tests.Extensions;

public class NullableExtensionsTests
{
	[Fact]
	public void Let_Struct_Null_ActionNotInvoked()
	{
		int? value = null;
		bool invoked = false;
		value.Let(_ => invoked = true);
		invoked.Should().BeFalse();
	}

	[Fact]
	public void Let_Struct_NonNull_ActionInvokedWithValue()
	{
		int? value = 42;
		int captured = 0;
		value.Let(v => captured = v);
		captured.Should().Be(42);
	}

	[Fact]
	public void Let_Class_Null_ActionNotInvoked()
	{
		string? value = null;
		bool invoked = false;
		value.Let(_ => invoked = true);
		invoked.Should().BeFalse();
	}

	[Fact]
	public void Let_Class_NonNull_ActionInvokedWithValue()
	{
		string? value = "hello";
		string captured = "";
		value.Let(v => captured = v);
		captured.Should().Be("hello");
	}

	[Fact]
	public void LetIf_Struct_Null_ActionNotInvoked()
	{
		int? value = null;
		bool invoked = false;
		value.LetIf(_ => false, _ => invoked = true);
		invoked.Should().BeFalse();
	}

	[Fact]
	public void LetIf_Struct_ConditionFalse_ActionNotInvoked()
	{
		int? value = 42;
		bool invoked = false;
		value.LetIf(_ => false, _ => invoked = true);
		invoked.Should().BeFalse();
	}

	[Fact]
	public void LetIf_Struct_ConditionTrue_ActionInvoked()
	{
		int? value = 42;
		bool invoked = false;
		value.LetIf(_ => true, _ => invoked = true);
		invoked.Should().BeTrue();
	}

	[Fact]
	public void LetIf_Class_Null_ActionNotInvoked()
	{
		object? value = null;
		bool invoked = false;
		value.LetIf(_ => false, _ => invoked = true);
		invoked.Should().BeFalse();
	}

	[Fact]
	public void LetIf_Class_ConditionFalse_ActionNotInvoked()
	{
		object? value = new();
		bool invoked = false;
		value.LetIf(_ => false, _ => invoked = true);
		invoked.Should().BeFalse();
	}

	[Fact]
	public void LetIf_Class_ConditionTrue_ActionInvoked()
	{
		object? value = new();
		bool invoked = false;
		value.LetIf(_ => true, _ => invoked = true);
		invoked.Should().BeTrue();
	}

	private enum Color { Red, Green, Blue }

	[Fact]
	public void LetParseEnum_ValidString_InvokesWithParsedEnum()
	{
		Color? captured = null;
		"Green".LetParseEnum<Color>(c => captured = c);
		captured.Should().Be(Color.Green);
	}

	[Fact]
	public void LetParseEnum_InvalidString_ActionNotInvoked()
	{
		bool invoked = false;
		"Purple".LetParseEnum<Color>(_ => invoked = true);
		invoked.Should().BeFalse();
	}

	[Fact]
	public void LetParseEnum_Null_ActionNotInvoked()
	{
		bool invoked = false;
		string? value = null;
		value.LetParseEnum<Color>(_ => invoked = true);
		invoked.Should().BeFalse();
	}
}
