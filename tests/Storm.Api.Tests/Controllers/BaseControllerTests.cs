using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Storm.Api;
using Storm.Api.Controllers;
using Storm.Api.CQRS;
using Storm.Api.CQRS.Domains.Results;
using Storm.Api.CQRS.Exceptions;
using Storm.Api.Dtos;
using Storm.Api.Logs;
using Storm.Api.Logs.Interfaces;

namespace Storm.Api.Tests.Controllers;

public class BaseControllerTests
{
	private sealed class NullSink : ILogSink
	{
		public void Enqueue(LogLevel level, string entry) { }
	}

	private sealed class TestController(IServiceProvider services) : BaseController(services)
	{
		public new Task<IActionResult> WrapForError<T>(Func<Task<T>> executor) => base.WrapForError(executor);
		public new Task<IActionResult> WrapForErrorRaw(Func<Task<IActionResult>> executor) => base.WrapForErrorRaw(executor);
		public new Task<ActionResult<T>> InternalWrapForError<T>(Func<Task<T>> executor) => base.InternalWrapForError(executor);
		public new Task<IActionResult> Action<TAction, TParameter, TOutput>(TParameter p) where TAction : IAction<TParameter, TOutput> => base.Action<TAction, TParameter, TOutput>(p);
		public new Task<IActionResult> Action<TAction, TParameter>(TParameter p) where TAction : IAction<TParameter, Unit> => base.Action<TAction, TParameter>(p);
		public new Task<IActionResult> FileAction<TAction, TParameter>(TParameter p) where TAction : IAction<TParameter, ApiFileResult> => base.FileAction<TAction, TParameter>(p);
	}

	private static IServiceProvider Provider()
	{
		ServiceCollection sc = new();
		sc.AddSingleton<ILogService>(new LogService(_ => new NullSink(), LogLevel.Trace));
		return sc.BuildServiceProvider();
	}

	private static TestController NewController()
	{
		TestController c = new(Provider());
		c.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
		return c;
	}

	[Fact]
	public async Task WrapForError_ActionReturnsResponse_KeepsSuccessTrue()
	{
		TestController c = NewController();
		IActionResult r = await c.WrapForError(() => Task.FromResult(new Response()));
		OkObjectResult ok = r.Should().BeOfType<OkObjectResult>().Which;
		Response resp = ok.Value.Should().BeOfType<Response>().Which;
		resp.IsSuccess.Should().BeTrue();
	}

	[Fact]
	public async Task WrapForError_ActionReturnsNonResponse_WrapsInResponseT()
	{
		TestController c = NewController();
		IActionResult r = await c.WrapForError(() => Task.FromResult(42));
		OkObjectResult ok = r.Should().BeOfType<OkObjectResult>().Which;
		Response<int> resp = ok.Value.Should().BeOfType<Response<int>>().Which;
		resp.Data.Should().Be(42);
		resp.IsSuccess.Should().BeTrue();
	}

	[Fact]
	public async Task WrapForError_DomainHttpCodeException_ReturnsStatusCodeWithErrorBody()
	{
		TestController c = NewController();
		IActionResult r = await c.WrapForError<int>(() => throw new DomainHttpCodeException(HttpStatusCode.BadRequest, "CODE", "msg"));
		ObjectResult obj = r.Should().BeOfType<ObjectResult>().Which;
		obj.StatusCode.Should().Be(400);
		Response resp = obj.Value.Should().BeOfType<Response>().Which;
		resp.IsSuccess.Should().BeFalse();
		resp.ErrorCode.Should().Be("CODE");
	}

	[Fact]
	public async Task WrapForError_DomainHttpCodeException_NullErrorCode_UsesGenericFallback()
	{
		TestController c = NewController();
		IActionResult r = await c.WrapForError<int>(() => throw new DomainHttpCodeException(HttpStatusCode.BadRequest, "", ""));
		ObjectResult obj = (ObjectResult)r;
		Response resp = (Response)obj.Value!;
		obj.StatusCode.Should().Be(400);
		resp.ErrorCode.Should().Be("GENERIC_HTTP_ERROR");
	}

	[Fact]
	public async Task WrapForError_DomainException_Returns200WithErrorBody()
	{
		TestController c = NewController();
		IActionResult r = await c.WrapForError<int>(() => throw new DomainException("CODE", "msg"));
		OkObjectResult ok = r.Should().BeOfType<OkObjectResult>().Which;
		Response resp = ok.Value.Should().BeOfType<Response>().Which;
		resp.IsSuccess.Should().BeFalse();
		resp.ErrorCode.Should().Be("CODE");
	}

	[Fact]
	public async Task WrapForError_GenericException_Rethrows_InDevEnvironment()
	{
		EnvironmentHelper.Set(EnvironmentSlot.Dev);
		try
		{
			TestController c = NewController();
			Func<Task> act = () => c.WrapForError<int>(() => throw new InvalidOperationException("boom"));
			await act.Should().ThrowAsync<InvalidOperationException>();
		}
		finally
		{
			EnvironmentHelper.Set(EnvironmentSlot.Local);
		}
	}

	[Fact]
	public async Task WrapForError_GenericException_Returns500_InProduction()
	{
		EnvironmentHelper.Set(EnvironmentSlot.Prod);
		try
		{
			TestController c = NewController();
			IActionResult r = await c.WrapForError<int>(() => throw new InvalidOperationException("boom"));
			ObjectResult obj = r.Should().BeOfType<ObjectResult>().Which;
			obj.StatusCode.Should().Be(500);
		}
		finally
		{
			EnvironmentHelper.Set(EnvironmentSlot.Local);
		}
	}

	[Fact]
	public async Task WrapForErrorRaw_PassesIActionResultThrough()
	{
		TestController c = NewController();
		NoContentResult nc = new();
		IActionResult r = await c.WrapForErrorRaw(() => Task.FromResult<IActionResult>(nc));
		r.Should().BeSameAs(nc);
	}

	[Fact]
	public async Task InternalWrapForError_ReturnsActionResultT()
	{
		TestController c = NewController();
		ActionResult<int> r = await c.InternalWrapForError(() => Task.FromResult(7));
		OkObjectResult ok = r.Result.Should().BeOfType<OkObjectResult>().Which;
		ok.Value.Should().Be(7);
	}

	private sealed class UnitParam { }

	private sealed class UnitAction : IAction<UnitParam, Unit>
	{
		public Task<Unit> Execute(UnitParam parameter) => Task.FromResult(Unit.Default);
	}

	private sealed class IntParam { public int V; }

	private sealed class IntAction : IAction<IntParam, int>
	{
		public Task<int> Execute(IntParam parameter) => Task.FromResult(parameter.V + 1);
	}

	[Fact]
	public async Task Action_Unit_ExecutesResolvedActionAndWraps()
	{
		TestController c = NewController();
		IActionResult r = await c.Action<UnitAction, UnitParam>(new UnitParam());
		r.Should().BeOfType<OkObjectResult>();
	}

	[Fact]
	public async Task Action_TypedOutput_WrapsInResponseT()
	{
		TestController c = NewController();
		IActionResult r = await c.Action<IntAction, IntParam, int>(new IntParam { V = 41 });
		OkObjectResult ok = r.Should().BeOfType<OkObjectResult>().Which;
		Response<int> resp = ok.Value.Should().BeOfType<Response<int>>().Which;
		resp.Data.Should().Be(42);
	}

	private sealed class BytesAction : IAction<UnitParam, ApiFileResult>
	{
		public Task<ApiFileResult> Execute(UnitParam p) => Task.FromResult(ApiFileResult.Create([1, 2, 3], "image/png", "a.png"));
	}

	private sealed class BytesNoNameAction : IAction<UnitParam, ApiFileResult>
	{
		public Task<ApiFileResult> Execute(UnitParam p) => Task.FromResult(ApiFileResult.Create([1], "application/octet-stream", ""));
	}

	private sealed class StreamAction : IAction<UnitParam, ApiFileResult>
	{
		public Task<ApiFileResult> Execute(UnitParam p) => Task.FromResult(ApiFileResult.Create(new MemoryStream([1, 2]), "image/png", "b.png"));
	}

	[Fact]
	public async Task FileAction_ByteResult_ReturnsFileWithContentTypeAndFilename()
	{
		TestController c = NewController();
		IActionResult r = await c.FileAction<BytesAction, UnitParam>(new UnitParam());
		FileContentResult fc = r.Should().BeOfType<FileContentResult>().Which;
		fc.FileDownloadName.Should().Be("a.png");
		fc.ContentType.Should().Be("image/png");
	}

	[Fact]
	public async Task FileAction_ByteResult_NoFilename_OmitsFilename()
	{
		TestController c = NewController();
		IActionResult r = await c.FileAction<BytesNoNameAction, UnitParam>(new UnitParam());
		FileContentResult fc = r.Should().BeOfType<FileContentResult>().Which;
		fc.FileDownloadName.Should().BeEmpty();
	}

	[Fact]
	public async Task FileAction_StreamResult_ReturnsFileStreamResult()
	{
		TestController c = NewController();
		IActionResult r = await c.FileAction<StreamAction, UnitParam>(new UnitParam());
		r.Should().BeOfType<FileStreamResult>();
	}
}
