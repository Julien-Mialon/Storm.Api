using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Storm.Api.Core;
using Storm.Api.Core.CQRS;
using Storm.Api.Core.Exceptions;
using Storm.Api.Core.Extensions;
using Storm.Api.Core.Logs;
using Storm.Api.Dtos;
using Storm.Api.Extensions;
using FileResult = Storm.Api.Core.Domains.Results.FileResult;

namespace Storm.Api.Controllers;

public abstract class BaseController : Controller
{
	/// <summary>
	/// Service collection to resolve every service from
	/// </summary>
	protected IServiceProvider Services { get; }

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="services">Injected service collection from AspNet.Core</param>
	protected BaseController(IServiceProvider services)
	{
		Services = services;
	}

	protected Task<IActionResult> WrapForError<T>(Func<Task<T>> executor)
	{
		return WrapForErrorRaw(async () =>
		{
			T result = await executor();

			if (result is Response response)
			{
				response.IsSuccess = true;
				return Ok(response);
			}

			return Ok(new Response<T>
			{
				Data = result,
				IsSuccess = true,
			});
		});
	}

	protected async Task<IActionResult> WrapForErrorRaw(Func<Task<IActionResult>> executor)
	{
		try
		{
			return await executor();
		}
		catch (DomainHttpCodeException ex)
		{
			Services.GetRequiredService<ILogService>().Warning(x => x
				.WriteProperty("message", $"DomainHttpCodeException: Code={ex.Code}, Message={ex.Message}")
				.WriteProperty("status", ex.Code)
				.WriteProperty("exceptionMessage", ex.Message)
				.WriteProperty("errorCode", ex.ErrorCode)
				.WriteProperty("errorMessage", ex.ErrorMessage)
				.WriteException(ex.InnerException, "inner")
			);

			return StatusCode(ex.Code, new Response
			{
				IsSuccess = false,
				ErrorCode = ex.ErrorCode.NullIfEmpty().ValueIfNull("GENERIC_HTTP_ERROR"),
				ErrorMessage = ex.ErrorMessage
			});
		}
		catch (DomainException ex)
		{
			Services.GetRequiredService<ILogService>().Warning(x => x
				.WriteProperty("message", $"DomainException: ErrorCode={ex.ErrorCode}, ErrorMessage={ex.ErrorMessage}")
				.WriteProperty("errorCode", ex.ErrorCode)
				.WriteProperty("errorMessage", ex.ErrorMessage)
				.WriteException(ex.InnerException, "inner")
			);

			return Ok(new Response
			{
				IsSuccess = false,
				ErrorCode = ex.ErrorCode,
				ErrorMessage = ex.ErrorMessage
			});
		}
		catch (Exception ex)
		{
			Services.GetRequiredService<ILogService>().Critical(x => x
				.WriteException(ex)
				.WriteProperty("controller", GetType().FullName)
			);

			if (!EnvironmentHelper.IsAvailableClient)
			{
				throw;
			}

			return StatusCode(500, new Response
			{
				IsSuccess = false,
				ErrorCode = "GENERIC_HTTP_ERROR",
				ErrorMessage = $"Exception: {ex}"
			});
		}
	}

	/// <summary>
	/// Execute an action on desired parameters
	/// </summary>
	protected Task<IActionResult> Action<TAction, TParameter>(TParameter parameter)
		where TAction : IAction<TParameter, Unit>
	{
		return Action<TAction, TParameter, Unit>(parameter);
	}

	/// <summary>
	/// Execute an action on desired parameters
	/// </summary>
	/// <typeparam name="TAction"></typeparam>
	/// <typeparam name="TParameter"></typeparam>
	/// <typeparam name="TOutput"></typeparam>
	/// <param name="parameter"></param>
	/// <returns></returns>
	protected Task<IActionResult> Action<TAction, TParameter, TOutput>(TParameter parameter)
		where TAction : IAction<TParameter, TOutput>
	{
		return WrapForError(() => Services.ExecuteAction<TAction, TParameter, TOutput>(parameter));
	}

	/// <summary>
	/// Execute a file action with parameters
	/// </summary>
	/// <typeparam name="TAction"></typeparam>
	/// <typeparam name="TParameter"></typeparam>
	/// <param name="parameter"></param>
	/// <returns></returns>
	protected Task<IActionResult> FileAction<TAction, TParameter>(TParameter parameter)
		where TAction : IAction<TParameter, FileResult>
	{
		return WrapForErrorRaw(async () =>
		{
			FileResult result = await Services.ExecuteAction<TAction, TParameter, FileResult>(parameter);
			if (result.FileName.IsNullOrEmpty())
			{
				if (result.IsRawData)
				{
					return File(result.AsRawData(), result.ContentType);
				}

				if(result.IsStreamData)
				{
					return File(result.AsStreamData(), result.ContentType);
				}

				throw new InvalidOperationException("Not supported file data format");
			}

			if (result.IsRawData)
			{
				return File(result.AsRawData(), result.ContentType, result.FileName);
			}

			if(result.IsStreamData)
			{
				return File(result.AsStreamData(), result.ContentType, result.FileName);
			}

			throw new InvalidOperationException("Not supported file data format");
		});
	}


}