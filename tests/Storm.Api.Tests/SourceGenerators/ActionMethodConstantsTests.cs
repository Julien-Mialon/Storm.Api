using System.Reflection;
using Storm.Api.SourceGenerators.Bases;

namespace Storm.Api.Tests.SourceGenerators;

public class ActionMethodConstantsTests
{
	private static AttributeDefinition GetAttribute(string fieldName)
	{
		Type t = typeof(BaseCodeGenerator).Assembly.GetType("Storm.Api.SourceGenerators.ActionMethods.ActionMethod")!;
		FieldInfo f = t.GetField(fieldName, BindingFlags.Static | BindingFlags.Public)!;
		return (AttributeDefinition)f.GetValue(null)!;
	}

	[Fact]
	public void WithAction_TargetsMethod_HasOneGenericParameter()
	{
		AttributeDefinition attr = GetAttribute("WITH_ACTION_ATTRIBUTE");
		attr.Targets.Should().Be(AttributeTargets.Method);
		attr.Generics.Should().ContainSingle().Which.Name.Should().Be("TAction");
	}

	[Fact]
	public void MapTo_TargetsParameter_HasFieldNameStringInConstructor()
	{
		AttributeDefinition attr = GetAttribute("MAP_TO_ATTRIBUTE");
		attr.Targets.Should().Be(AttributeTargets.Parameter);
		AttributePropertyDefinition prop = attr.Properties.Should().ContainSingle().Which;
		prop.Name.Should().Be("FieldName");
		prop.Type.Should().Be(typeof(string));
		prop.InConstructor.Should().BeTrue();
	}
}
