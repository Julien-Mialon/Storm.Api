using JsonSerializer = System.Text.Json.JsonSerializer;
using Newtonsoft.Json;
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
	public void PaginatedResponse_Serialize_SystemTextJson_IncludesPageCountTotalCountData()
	{
		PaginatedResponse<int> p = new() { Page = 1, Count = 10, TotalCount = 100, Data = [1, 2, 3] };
		string json = JsonSerializer.Serialize(p);
		json.Should().Contain("\"page\":1");
		json.Should().Contain("\"count\":10");
		json.Should().Contain("\"total_count\":100");
		json.Should().Contain("\"data\":[1,2,3]");
	}

	[Fact]
	public void PaginatedResponse_Serialize_NewtonsoftJson_IncludesPageCountTotalCountData()
	{
		PaginatedResponse<int> p = new() { Page = 1, Count = 10, TotalCount = 100, Data = [1, 2, 3] };
		string json = JsonConvert.SerializeObject(p);
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

	[Fact]
	public void PaginatedResponse_InheritsIsSuccessAndErrorFields_Serialize_NewtonsoftJson()
	{
		PaginatedResponse<int> p = new() { IsSuccess = true, ErrorCode = "x", ErrorMessage = "m", Data = [1] };
		string json = JsonConvert.SerializeObject(p);
		json.Should().Contain("\"is_success\":true");
		json.Should().Contain("\"error_code\":\"x\"");
		json.Should().Contain("\"error_message\":\"m\"");
	}

	[Fact]
	public void PaginatedResponse_DeserializeRoundtrip_SystemTextJson()
	{
		PaginatedResponse<int> p = new() { Page = 2, Count = 5, TotalCount = 42, Data = [10, 20], IsSuccess = true };
		string json = JsonSerializer.Serialize(p);
		PaginatedResponse<int>? back = JsonSerializer.Deserialize<PaginatedResponse<int>>(json);
		back.Should().NotBeNull();
		back!.Page.Should().Be(2);
		back.Count.Should().Be(5);
		back.TotalCount.Should().Be(42);
		back.Data.Should().Equal(10, 20);
		back.IsSuccess.Should().BeTrue();
	}

	[Fact]
	public void PaginatedResponse_DeserializeRoundtrip_NewtonsoftJson()
	{
		PaginatedResponse<int> p = new() { Page = 2, Count = 5, TotalCount = 42, Data = [10, 20], IsSuccess = true };
		string json = JsonConvert.SerializeObject(p);
		PaginatedResponse<int>? back = JsonConvert.DeserializeObject<PaginatedResponse<int>>(json);
		back.Should().NotBeNull();
		back!.Page.Should().Be(2);
		back.Count.Should().Be(5);
		back.TotalCount.Should().Be(42);
		back.Data.Should().Equal(10, 20);
		back.IsSuccess.Should().BeTrue();
	}
}
