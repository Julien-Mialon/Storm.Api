using Storm.Api.SourceGenerators.Bases;

namespace Storm.Api.SourceGenerators.ActionMethods;

internal static class ActionMethod
{
	public static readonly AttributeDefinition WITH_ACTION_ATTRIBUTE = new()
	{
		Namespace = ActionMethodCodeGenerator.GeneratedNamespace,
		Name = "WithAction",
		Targets = AttributeTargets.Method,
		Generics = [
			new()
			{
				Name = "TAction"
			}
		]
	};

	public static readonly AttributeDefinition MAP_TO_ATTRIBUTE = new()
	{
		Namespace = ActionMethodCodeGenerator.GeneratedNamespace,
		Name = "MapTo",
		Targets = AttributeTargets.Parameter,
		Properties =
		[
			new()
			{
				Name = "FieldName",
				Type = typeof(string),
				InConstructor = true,
			}
		]
	};
}