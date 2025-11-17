namespace Storm.Api.SourceGenerators.Bases;

public class GenericConstraintDefinition
{
	public bool IsClass { get; set; }
	public bool IsStruct { get; set; }
	public bool HasNew { get; set; }
	public List<string> TypeConstraints { get; set; } = [];
}