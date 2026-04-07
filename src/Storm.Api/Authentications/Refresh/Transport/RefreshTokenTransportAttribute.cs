using Microsoft.AspNetCore.Mvc.Filters;

namespace Storm.Api.Authentications.Refresh.Transport;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RefreshTokenTransportAttribute : ActionFilterAttribute
{
	internal const string HTTP_CONTEXT_KEY = "Storm.RefreshTokenTransportMode";

	public RefreshTokenTransportMode Mode { get; }

	public RefreshTokenTransportAttribute(RefreshTokenTransportMode mode)
	{
		Mode = mode;
	}

	public override void OnActionExecuting(ActionExecutingContext context)
	{
		context.HttpContext.Items[HTTP_CONTEXT_KEY] = Mode;
		base.OnActionExecuting(context);
	}
}
