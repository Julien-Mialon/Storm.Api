using JsonSerializer = System.Text.Json.JsonSerializer;
using Newtonsoft.Json;
using Storm.Api.Dtos;

namespace Storm.Api.Tests.Dtos;

public class ResponseTests
{
	[Fact]
	public void Response_DefaultValues_IsSuccessFalse_NullErrorFields()
	{
		Response r = new();
		r.IsSuccess.Should().BeFalse();
		r.ErrorCode.Should().BeNull();
		r.ErrorMessage.Should().BeNull();
	}

	[Fact]
	public void Response_Serialize_SystemTextJson_UsesSnakeCasePropertyNames()
	{
		Response r = new() { IsSuccess = true, ErrorCode = "c", ErrorMessage = "m" };
		string json = JsonSerializer.Serialize(r);
		json.Should().Contain("\"is_success\":true");
		json.Should().Contain("\"error_code\":\"c\"");
		json.Should().Contain("\"error_message\":\"m\"");
	}

	[Fact]
	public void Response_Serialize_NewtonsoftJson_UsesSnakeCasePropertyNames()
	{
		Response r = new() { IsSuccess = true, ErrorCode = "c", ErrorMessage = "m" };
		string json = JsonConvert.SerializeObject(r);
		json.Should().Contain("\"is_success\":true");
		json.Should().Contain("\"error_code\":\"c\"");
		json.Should().Contain("\"error_message\":\"m\"");
	}

	[Fact]
	public void ResponseT_SerializeWithData_IncludesDataField()
	{
		Response<int> r = new() { Data = 42, IsSuccess = true };
		string json = JsonSerializer.Serialize(r);
		json.Should().Contain("\"data\":42");
	}

	[Fact]
	public void ResponseT_SerializeNullData_EmitsNull()
	{
		Response<string> r = new() { Data = null };
		string json = JsonSerializer.Serialize(r);
		json.Should().Contain("\"data\":null");
	}

	[Fact]
	public void Response_DeserializeRoundtrip_PreservesAllFields()
	{
		Response<int> r = new() { IsSuccess = true, ErrorCode = "c", ErrorMessage = "m", Data = 7 };
		string json = JsonSerializer.Serialize(r);
		Response<int>? back = JsonSerializer.Deserialize<Response<int>>(json);
		back.Should().NotBeNull();
		back!.IsSuccess.Should().BeTrue();
		back.ErrorCode.Should().Be("c");
		back.ErrorMessage.Should().Be("m");
		back.Data.Should().Be(7);
	}
}
