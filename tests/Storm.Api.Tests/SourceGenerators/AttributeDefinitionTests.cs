using Storm.Api.SourceGenerators.Bases;

namespace Storm.Api.Tests.SourceGenerators;

public class AttributeDefinitionTests
{
	[Fact]
	public void MetadataName_CombinesNamespaceAndName()
	{
		AttributeDefinition def = new() { Name = "With", Targets = AttributeTargets.Class };
		def.Namespace = "My.NS";
		def.MetadataName.Should().Be("My.NS.WithAttribute");
	}

	[Fact]
	public void FullName_EmittedAsExpected()
	{
		AttributeDefinition def = new() { Name = "WithAction", Targets = AttributeTargets.Method };
		def.FullName.Should().Be("WithActionAttribute");
	}

	[Fact]
	public void Generics_StoredInOrder()
	{
		AttributeDefinition def = new()
		{
			Name = "X",
			Targets = AttributeTargets.Class,
			Generics = [new AttributeGenericDefinition { Name = "T1" }, new AttributeGenericDefinition { Name = "T2" }],
		};
		def.Generics.Select(g => g.Name).Should().Equal("T1", "T2");
	}

	[Fact]
	public void Properties_StoredInOrder()
	{
		AttributeDefinition def = new()
		{
			Name = "X",
			Targets = AttributeTargets.Class,
			Properties =
			[
				new AttributePropertyDefinition { Name = "First", Type = typeof(string) },
				new AttributePropertyDefinition { Name = "Second", Type = typeof(int) },
			],
		};
		def.Properties.Select(p => p.Name).Should().Equal("First", "Second");
	}

	[Fact]
	public void MetadataName_WithGenerics_IncludesArityMarker()
	{
		AttributeDefinition def = new()
		{
			Name = "WithAction",
			Targets = AttributeTargets.Method,
			Generics = [new AttributeGenericDefinition { Name = "T" }],
		};
		def.Namespace = "My.NS";
		def.MetadataName.Should().Be("My.NS.WithActionAttribute`1");
	}

	[Fact]
	public void AttributePropertyDefinition_InConstructorTrue_SetsCamelCaseConstructorArgName()
	{
		AttributePropertyDefinition p = new() { Name = "FieldName", Type = typeof(string), InConstructor = true };
		p.ConstructorArgumentName.Should().Be("fieldName");
	}

	[Fact]
	public void AttributePropertyDefinition_ConstructorArgName_AlwaysCamelCase()
	{
		AttributePropertyDefinition p = new() { Name = "FieldName", Type = typeof(string), InConstructor = false };
		p.ConstructorArgumentName.Should().Be("fieldName");
	}

	[Fact]
	public void GenericConstraintDefinition_IsClass_Flag()
	{
		GenericConstraintDefinition g = new() { IsClass = true };
		g.IsClass.Should().BeTrue();
		g.IsStruct.Should().BeFalse();
	}

	[Fact]
	public void GenericConstraintDefinition_IsStruct_Flag()
	{
		GenericConstraintDefinition g = new() { IsStruct = true };
		g.IsStruct.Should().BeTrue();
	}

	[Fact]
	public void GenericConstraintDefinition_HasNew_Flag()
	{
		GenericConstraintDefinition g = new() { HasNew = true };
		g.HasNew.Should().BeTrue();
	}

	[Fact]
	public void GenericConstraintDefinition_TypeConstraints_Preserved()
	{
		GenericConstraintDefinition g = new() { TypeConstraints = ["IFoo", "IBar"] };
		g.TypeConstraints.Should().Equal("IFoo", "IBar");
	}
}
