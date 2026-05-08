using System.Net;
using Storm.Api.SourceGenerators.Bases;

namespace Storm.Api.SourceGenerators.ActionMethods;

internal static class ActionMethod
{
	public static readonly AttributeDefinition WITH_ACTION_ATTRIBUTE = new()
	{
		Namespace = ActionMethodCodeGenerator.GeneratedNamespace,
		Name = "WithAction",
		Targets = AttributeTargets.Method,
		Generics =
		[
			new()
			{
				Name = "TAction",
			},
		],
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
			},
		],
	};

	public static readonly AttributeDefinition SUCCESS_CODE_ATTRIBUTE = new()
	{
		Namespace = ActionMethodCodeGenerator.GeneratedNamespace,
		Name = "SuccessCode",
		Targets = AttributeTargets.Class,
		AllowMultiple = true,
		Properties =
		[
			new()
			{
				Name = "Code",
				Type = typeof(string),
				InConstructor = true,
			},
			new()
			{
				Name = "Description",
				Type = typeof(string),
				InConstructor = false,
				IsNullable = true,
			}
		],
	};

	public static readonly AttributeDefinition MEDIA_TYPE_ATTRIBUTE = new()
	{
		Namespace = ActionMethodCodeGenerator.GeneratedNamespace,
		Name = "MediaType",
		Targets = AttributeTargets.Class,
		AllowMultiple = false,
		Properties =
		[
			new()
			{
				Name = "Type",
				Type = typeof(string),
				InConstructor = true,
			},
		],
	};

	public static readonly AttributeDefinition ERROR_CODE_ATTRIBUTE = new()
	{
		Namespace = ActionMethodCodeGenerator.GeneratedNamespace,
		Name = "ErrorCode",
		Targets = AttributeTargets.Class,
		AllowMultiple = true,
		Properties =
		[
			new()
			{
				Name = "Code",
				Type = typeof(string),
				InConstructor = true,
			},
			new()
			{
				Name = "Description",
				Type = typeof(string),
				InConstructor = false,
				IsNullable = true,
			}
		],
	};

	public static readonly AttributeDefinition HTTP_ERROR_ATTRIBUTE = new()
	{
		Namespace = ActionMethodCodeGenerator.GeneratedNamespace,
		Name = "HttpError",
		Targets = AttributeTargets.Class,
		AllowMultiple = true,
		Properties =
		[
			new()
			{
				Name = "Code",
				Type = typeof(HttpStatusCode),
				InConstructor = true,
			},
			new()
			{
				Name = "Description",
				Type = typeof(string),
				InConstructor = false,
				IsNullable = true,
			}
		],
	};

	public static readonly AttributeDefinition DESCRIPTION_ATTRIBUTE = new()
	{
		Namespace = ActionMethodCodeGenerator.GeneratedNamespace,
		Name = "Description",
		Targets = AttributeTargets.Class,
		AllowMultiple = true,
		Properties =
		[
			new()
			{
				Name = "Text",
				Type = typeof(string),
				InConstructor = true,
			},
		],
	};

	public static readonly AttributeDefinition SUMMARY_ATTRIBUTE = new()
	{
		Namespace = ActionMethodCodeGenerator.GeneratedNamespace,
		Name = "Summary",
		Targets = AttributeTargets.Class,
		AllowMultiple = true,
		Properties =
		[
			new()
			{
				Name = "Text",
				Type = typeof(string),
				InConstructor = true,
			},
		],
	};

	public static readonly AttributeDefinition INTERNAL_ACTION_CALL_ATTRIBUTE = new()
	{
		Namespace = ActionMethodCodeGenerator.GeneratedNamespace,
		Name = "InternalActionCall",
		Targets = AttributeTargets.Class,
		AllowMultiple = true,
		Generics =
		[
			new()
			{
				Name = "TAction",
			},
		],
	};
}