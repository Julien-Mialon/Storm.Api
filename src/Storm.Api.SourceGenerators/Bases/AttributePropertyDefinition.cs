namespace Storm.Api.SourceGenerators.Bases;

public class AttributePropertyDefinition
{
	public required Type Type { get; set; }
	public required string Name { get; set; }
	public bool InConstructor { get; set; }

	public string ConstructorArgumentName => $"{(char)(Name[0] ^ 32)}{Name[1..]}";
}