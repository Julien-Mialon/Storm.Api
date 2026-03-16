using Microsoft.AspNetCore.Mvc.Filters;

namespace Storm.Api.Authentications.Refresh;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RefreshTokenModeAttribute : ActionFilterAttribute
{
	internal const string HttpContextKey = "Storm.RefreshTokenMode";

	public RefreshTokenMode Mode { get; }

	public RefreshTokenModeAttribute(RefreshTokenMode mode)
	{
		Mode = mode;
	}

	public override void OnActionExecuting(ActionExecutingContext context)
	{
		context.HttpContext.Items[HttpContextKey] = Mode;
		base.OnActionExecuting(context);
	}
}