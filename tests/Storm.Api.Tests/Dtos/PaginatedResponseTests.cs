using System.Text.Json;
using Storm.Api.Dtos;

namespace Storm.Api.Tests.Dtos;

public class PaginatedResponseTests
{
	[Fact]
	public void PaginatedResponse_Defaults_AllNumericFieldsZero_DataNull()
	{
		PaginatedResponse<int> p = new();
		p.Page.Should().Be(0);
		p.Count.Should().Be(0);
		p.TotalCount.Should().Be(0);
		p.Data.Should().BeNull();
	}

	[Fact]
	public void PaginatedResponse_Serialize_IncludesPageCountTotalCountData()
	{
		PaginatedResponse<int> p = new() { Page = 1, Count = 10, TotalCount = 100, Data = [1, 2, 3] };
		string json = JsonSerializer.Serialize(p);
		json.Should().Contain("\"page\":1");
		json.Should().Contain("\"count\":10");
		json.Should().Contain("\"total_count\":100");
		json.Should().Contain("\"data\":[1,2,3]");
	}

	[Fact]
	public void PaginatedResponse_EmptyArray_SerializesAsEmptyArray()
	{
		PaginatedResponse<int> p = new() { Data = [] };
		string json = JsonSerializer.Serialize(p);
		json.Should().Contain("\"data\":[]");
	}

	[Fact]
	public void PaginatedResponse_InheritsIsSuccessAndErrorFields()
	{
		PaginatedResponse<int> p = new() { IsSuccess = true, ErrorCode = "x" };
		p.Should().BeAssignableTo<Response>();
		p.IsSuccess.Should().BeTrue();
		p.ErrorCode.Should().Be("x");
	}
}
