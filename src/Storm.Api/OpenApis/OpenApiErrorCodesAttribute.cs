namespace Storm.Api.OpenApis;

[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
public sealed class OpenApiErrorCodesAttribute : Attribute
{
	public string[] Codes { get; }

	public OpenApiErrorCodesAttribute(string code, params string[] codes)
	{
		Codes = [code, ..codes];
	}
}