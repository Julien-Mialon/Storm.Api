using System.Reflection;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite;
using Storm.Api.Databases.Models;

namespace Storm.Api.Tests.Databases;

public class SqlFieldsOrderingTests
{
	public class SampleEntity : BaseDeletableEntity
	{
		public string? Name { get; set; }
		public string? Description { get; set; }
	}

	public class NoIdEntity
	{
		public string? Foo { get; set; }
		public string? Bar { get; set; }
	}

	private static List<string> GetReorderedFieldNames(Type entityType)
	{
		ModelDefinition modelDef = ModelDefinition<object>.Definition;
		MethodInfo? getModelDef = typeof(OrmLiteReadApi).Assembly
			.GetType("ServiceStack.OrmLite.OrmLiteUtils")
			?.GetMethod("GetModelDefinition", BindingFlags.Public | BindingFlags.Static);
		if (getModelDef != null)
		{
			modelDef = (ModelDefinition)getModelDef.Invoke(null, [entityType])!;
		}
		else
		{
			Type defType = typeof(ModelDefinition<>).MakeGenericType(entityType);
			modelDef = (ModelDefinition)defType.GetProperty("Definition", BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;
		}

		Type ordering = typeof(Storm.Api.Databases.Services.BaseDatabaseService).Assembly
			.GetType("Storm.Api.Databases.Internals.SqlFieldsOrdering")!;
		MethodInfo reorder = ordering.GetMethod("ReorderField", BindingFlags.NonPublic | BindingFlags.Static)!;
		reorder.Invoke(null, [modelDef]);
		return modelDef.FieldDefinitions.Select(f => f.Name).ToList();
	}

	[Fact]
	public void ReorderField_IdFieldMovedToFront()
	{
		List<string> names = GetReorderedFieldNames(typeof(SampleEntity));
		names[0].Should().Be("Id");
	}

	[Fact]
	public void ReorderField_SoftDeleteFieldsMovedToEnd()
	{
		List<string> names = GetReorderedFieldNames(typeof(SampleEntity));
		names.Last().Should().Be("EntityDeletedDate");
		names.Should().EndWith(["IsDeleted", "EntityCreatedDate", "EntityUpdatedDate", "EntityDeletedDate"]);
	}

	[Fact]
	public void ReorderField_BusinessFieldsPreserveOriginalOrder()
	{
		List<string> names = GetReorderedFieldNames(typeof(SampleEntity));
		int nameIdx = names.IndexOf("Name");
		int descIdx = names.IndexOf("Description");
		nameIdx.Should().BeLessThan(descIdx);
	}

	[Fact]
	public void ReorderField_NoIdField_HandledGracefully()
	{
		List<string> names = GetReorderedFieldNames(typeof(NoIdEntity));
		names.Should().Contain(["Foo", "Bar"]);
	}
}
