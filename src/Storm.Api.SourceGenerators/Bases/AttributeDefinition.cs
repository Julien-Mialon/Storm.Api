namespace Storm.Api.SourceGenerators.Bases;

public class AttributeDefinition
{
	public string Namespace
	{
		get;
		set
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				value = "Storm.Api.SourceGenerators.Generated";
			}

			field = value;
		}
	} = "Storm.Api.SourceGenerators.Generated";

	public required string Name { get; set; }
	public required AttributeTargets Targets { get; set; }
	public bool AllowMultiple { get; set; }
	public bool Inherited { get; set; }
	public List<AttributePropertyDefinition> Properties { get; set; } = [];
	public List<AttributeGenericDefinition> Generics { get; set; } = [];

	public string FullName => $"{Name}Attribute";
	public string MetadataName
	{
		get
		{
			string baseName = $"{Namespace}.{FullName}";
			if (Generics.Count > 0)
			{
				baseName += $"`{Generics.Count}";
			}
			return baseName;
		}
	}
}