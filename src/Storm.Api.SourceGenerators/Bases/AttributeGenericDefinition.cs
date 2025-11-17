namespace Storm.Api.SourceGenerators.Bases;

public class AttributeGenericDefinition
{
	public required string Name { get; set; }
	public GenericConstraintDefinition? Constraints { get; set; }
}