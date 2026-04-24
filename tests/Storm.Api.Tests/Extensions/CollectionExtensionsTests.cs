using Storm.Api.Extensions;

namespace Storm.Api.Tests.Extensions;

public class CollectionExtensionsTests
{
	[Fact]
	public void ConvertAll_Enumerable_NullSource_Throws()
	{
		IEnumerable<int> source = null!;
		Action act = () => source.ConvertAll(x => x);
		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void ConvertAll_Enumerable_AppliesMapperToEach()
	{
		IEnumerable<int> source = Enumerable.Range(1, 3);
		source.ConvertAll(x => x * 2).Should().Equal(2, 4, 6);
	}

	[Fact]
	public void ConvertAll_Collection_PreallocatesListSize()
	{
		ICollection<int> source = new List<int> { 1, 2, 3, 4 };
		List<string> result = source.ConvertAll(x => x.ToString());
		result.Should().Equal("1", "2", "3", "4");
	}

	[Fact]
	public async Task ConvertAll_TaskOverload_AwaitsThenMaps()
	{
		Task<IEnumerable<int>> source = Task.FromResult<IEnumerable<int>>(new[] { 1, 2 });
		List<int> result = await source.ConvertAll(x => x + 10);
		result.Should().Equal(11, 12);
	}

	[Fact]
	public void None_EmptyEnumerable_ReturnsTrue()
	{
		Enumerable.Empty<int>().None().Should().BeTrue();
	}

	[Fact]
	public void None_NonEmptyEnumerable_ReturnsFalse()
	{
		new[] { 1 }.AsEnumerable().None().Should().BeFalse();
	}

	[Fact]
	public void None_WithPredicate_NoMatch_ReturnsTrue()
	{
		new[] { 1, 2, 3 }.None(x => x > 10).Should().BeTrue();
	}

	[Fact]
	public void None_WithPredicate_AnyMatch_ReturnsFalse()
	{
		new[] { 1, 2, 3 }.None(x => x == 2).Should().BeFalse();
	}

	[Fact]
	public void AddRange_HashSet_AddsAllItems_SkippingDuplicates()
	{
		HashSet<int> set = [1, 2];
		set.AddRange([2, 3, 4]);
		set.Should().BeEquivalentTo(new[] { 1, 2, 3, 4 });
	}

	[Fact]
	public void ToSafeDictionary_NullSource_ReturnsEmptyDictionary()
	{
		IEnumerable<int>? source = null;
		Dictionary<int, int> result = source.ToSafeDictionary(x => x, x => x);
		result.Should().BeEmpty();
	}

	[Fact]
	public void ToSafeDictionary_BuildsDictionaryFromSelectors()
	{
		int[] source = [1, 2, 3];
		Dictionary<int, string> result = source.ToSafeDictionary(x => x, x => x.ToString());
		result.Should().Equal(new Dictionary<int, string> { { 1, "1" }, { 2, "2" }, { 3, "3" } });
	}

	[Fact]
	public void ToListOrDefault_Null_ReturnsEmptyList()
	{
		IEnumerable<int>? source = null;
		source.ToListOrDefault().Should().BeEmpty();
	}

	[Fact]
	public void ToListOrDefault_NonNull_ReturnsListWithItems()
	{
		new[] { 1, 2 }.AsEnumerable().ToListOrDefault().Should().Equal(1, 2);
	}

	[Fact]
	public void IndexOfMin_EmptyList_Throws()
	{
		IReadOnlyList<int> source = new List<int>();
		Action act = () => source.IndexOfMin(x => x);
		act.Should().Throw<ArgumentOutOfRangeException>();
	}

	[Fact]
	public void IndexOfMin_ReturnsFirstIndexOnTies()
	{
		IReadOnlyList<int> source = [5, 3, 3, 1, 1];
		source.IndexOfMin(x => x).Should().Be(3);
	}

	[Fact]
	public void IndexOfMin_UsesCustomComparer()
	{
		IReadOnlyList<int> source = [1, 2, 3];
		int index = source.IndexOfMin(x => x, Comparer<int>.Create((a, b) => b.CompareTo(a)));
		index.Should().Be(2);
	}

	[Fact]
	public void IndexOfMax_EmptyList_Throws()
	{
		IReadOnlyList<int> source = new List<int>();
		Action act = () => source.IndexOfMax(x => x);
		act.Should().Throw<ArgumentOutOfRangeException>();
	}

	[Fact]
	public void IndexOfMax_ReturnsFirstIndexOnTies()
	{
		IReadOnlyList<int> source = [1, 5, 3, 5];
		source.IndexOfMax(x => x).Should().Be(1);
	}

	[Fact]
	public void IndexOfMax_UsesCustomComparer()
	{
		IReadOnlyList<int> source = [1, 2, 3];
		int index = source.IndexOfMax(x => x, Comparer<int>.Create((a, b) => b.CompareTo(a)));
		index.Should().Be(0);
	}
}
