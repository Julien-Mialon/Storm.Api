namespace Storm.Api.SourceGenerators.Tests.Infrastructure;

/// <summary>
/// Minimal, self-contained source for the Storm.Api / ASP.NET Core types that the generator looks
/// up by metadata name and that the generated code is compiled against.
/// <para>
/// Keeping these as source (instead of referencing the real assemblies + the ASP.NET shared
/// framework) makes the generator tests deterministic and fast: the only moving parts are the
/// generator itself and the per-test controller/action source.
/// </para>
/// <para>
/// The signatures mirror the real framework exactly where the generated code relies on them
/// (notably <c>ExecuteAction</c>, <c>InternalWrapForError</c> and <c>FileAction</c> generic
/// constraints) so that "the generated code compiles" is a meaningful assertion.
/// </para>
/// </summary>
internal static class FrameworkStubs
{
	public const string SOURCE = """

		namespace Storm.Api
		{
			public sealed class Unit
			{
				public static readonly Unit Default = new Unit();
				private Unit() { }
			}
		}

		namespace Storm.Api.Dtos
		{
			public class Response
			{
				public bool IsSuccess { get; set; }
				public string? ErrorCode { get; set; }
				public string? ErrorMessage { get; set; }
			}

			public class Response<T> : Response
			{
				public T? Data { get; set; }
			}
		}

		namespace Storm.Api.CQRS
		{
			public interface IAction<in TParameter, TOutput>
			{
				System.Threading.Tasks.Task<TOutput> Execute(TParameter parameter);
			}

			public abstract class BaseAction<TParameter, TOutput> : IAction<TParameter, TOutput>
			{
				protected BaseAction(System.IServiceProvider services) { }

				public System.Threading.Tasks.Task<TOutput> Execute(TParameter parameter)
					=> throw new System.NotImplementedException();

				protected abstract System.Threading.Tasks.Task<TOutput> Action(TParameter parameter);
			}

			public abstract class BaseAuthenticatedAction<TParameter, TOutput, TAccount> : IAction<TParameter, TOutput>
			{
				protected BaseAuthenticatedAction(System.IServiceProvider services) { }

				public System.Threading.Tasks.Task<TOutput> Execute(TParameter parameter)
					=> throw new System.NotImplementedException();

				protected abstract System.Threading.Tasks.Task<TOutput> Action(TParameter parameter, TAccount account);
			}
		}

		namespace Storm.Api.CQRS.Domains.Results
		{
			public class ApiFileResult
			{
				public string? FileName { get; set; }
			}
		}

		namespace Storm.Api.OpenApis
		{
			[System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false)]
			public sealed class OpenApiErrorCodesAttribute : System.Attribute
			{
				public OpenApiErrorCodesAttribute(params string[] codes) { }
			}
		}

		namespace Storm.Api.Extensions
		{
			public static class ServicesExtensions
			{
				public static System.Threading.Tasks.Task<TOutput> ExecuteAction<TAction, TParameter, TOutput>(this System.IServiceProvider services, TParameter parameter)
					where TAction : Storm.Api.CQRS.IAction<TParameter, TOutput>
					=> throw new System.NotImplementedException();
			}
		}

		namespace Storm.Api.Controllers
		{
			public abstract class BaseController
			{
				protected System.IServiceProvider Services { get; }

				protected BaseController(System.IServiceProvider services)
				{
					Services = services;
				}

				protected System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.ActionResult<T>> InternalWrapForError<T>(System.Func<System.Threading.Tasks.Task<T>> executor)
					=> throw new System.NotImplementedException();

				protected System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.IActionResult> FileAction<TAction, TParameter>(TParameter parameter)
					where TAction : Storm.Api.CQRS.IAction<TParameter, Storm.Api.CQRS.Domains.Results.ApiFileResult>
					=> throw new System.NotImplementedException();
			}
		}

		namespace Microsoft.AspNetCore.Mvc
		{
			public interface IActionResult { }

			public class ActionResult<TValue>
			{
				public static implicit operator ActionResult<TValue>(TValue value) => new ActionResult<TValue>();
			}

			[System.AttributeUsage(System.AttributeTargets.Class)]
			public sealed class ApiControllerAttribute : System.Attribute { }

			public class HttpGetAttribute : System.Attribute
			{
				public HttpGetAttribute() { }
				public HttpGetAttribute(string template) { }
			}

			public class TagsAttribute : System.Attribute
			{
				public TagsAttribute(params string[] tags) { }
			}

			public class FromRouteAttribute : System.Attribute { public string? Name { get; set; } }
			public class FromQueryAttribute : System.Attribute { public string? Name { get; set; } }
			public class FromBodyAttribute : System.Attribute { }
			public class FromHeaderAttribute : System.Attribute { public string? Name { get; set; } }

			public class ProducesResponseTypeAttribute : System.Attribute
			{
				public ProducesResponseTypeAttribute(int statusCode) { }
			}

			[System.AttributeUsage(System.AttributeTargets.Method | System.AttributeTargets.Class, AllowMultiple = true)]
			public sealed class ProducesResponseTypeAttribute<T> : System.Attribute
			{
				public ProducesResponseTypeAttribute(int statusCode, string? contentType = null) { }
				public string? Description { get; set; }
			}
		}

		namespace Microsoft.AspNetCore.Http
		{
			public sealed class EndpointSummaryAttribute : System.Attribute
			{
				public EndpointSummaryAttribute(string summary) { }
			}

			public sealed class EndpointDescriptionAttribute : System.Attribute
			{
				public EndpointDescriptionAttribute(string description) { }
			}
		}

		""";
}
