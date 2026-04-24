using Storm.Api.Extensions;

namespace Storm.Api.Tests.Extensions;

public class NumberExtensionsTests
{
	[Fact]
	public void IsPositive_Zero_ReturnsTrue() => 0.IsPositive().Should().BeTrue();

	[Fact]
	public void IsPositive_Negative_ReturnsFalse() => (-1).IsPositive().Should().BeFalse();

	[Fact]
	public void IsStrictlyPositive_Zero_ReturnsFalse() => 0.IsStrictlyPositive().Should().BeFalse();

	[Fact]
	public void IsStrictlyPositive_Positive_ReturnsTrue() => 1.IsStrictlyPositive().Should().BeTrue();

	[Fact]
	public void IsNegative_Zero_ReturnsTrue() => 0.IsNegative().Should().BeTrue();

	[Fact]
	public void IsStrictlyNegative_Zero_ReturnsFalse() => 0.IsStrictlyNegative().Should().BeFalse();

	[Fact]
	public void IsNull_Null_ReturnsTrue()
	{
		int? a = null;
		long? b = null;
		float? c = null;
		double? d = null;
		decimal? e = null;
		a.IsNull().Should().BeTrue();
		b.IsNull().Should().BeTrue();
		c.IsNull().Should().BeTrue();
		d.IsNull().Should().BeTrue();
		e.IsNull().Should().BeTrue();
	}

	[Fact]
	public void IsNotNull_Value_ReturnsTrue()
	{
		int? a = 1;
		long? b = 1L;
		float? c = 1f;
		double? d = 1.0;
		decimal? e = 1m;
		a.IsNotNull().Should().BeTrue();
		b.IsNotNull().Should().BeTrue();
		c.IsNotNull().Should().BeTrue();
		d.IsNotNull().Should().BeTrue();
		e.IsNotNull().Should().BeTrue();
	}

	[Fact]
	public void FloatingPoint_NaN_BehavesAsImplemented()
	{
		float.NaN.IsPositive().Should().BeFalse();
		float.NaN.IsNegative().Should().BeFalse();
		double.NaN.IsPositive().Should().BeFalse();
		double.NaN.IsNegative().Should().BeFalse();
	}

	[Theory]
	[InlineData(0, true, false, true, false)]
	[InlineData(1, true, true, false, false)]
	[InlineData(-1, false, false, true, true)]
	public void AllNumericTypes_DataDriven_BehaveIdentically(int value, bool pos, bool strictPos, bool neg, bool strictNeg)
	{
		value.IsPositive().Should().Be(pos);
		value.IsStrictlyPositive().Should().Be(strictPos);
		value.IsNegative().Should().Be(neg);
		value.IsStrictlyNegative().Should().Be(strictNeg);

		((long)value).IsPositive().Should().Be(pos);
		((long)value).IsStrictlyPositive().Should().Be(strictPos);
		((long)value).IsNegative().Should().Be(neg);
		((long)value).IsStrictlyNegative().Should().Be(strictNeg);

		((float)value).IsPositive().Should().Be(pos);
		((double)value).IsPositive().Should().Be(pos);
		((decimal)value).IsPositive().Should().Be(pos);
	}
}
