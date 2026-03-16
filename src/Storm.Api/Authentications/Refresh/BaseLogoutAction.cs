using Storm.Api.CQRS;

namespace Storm.Api.Authentications.Refresh;

public class BaseLogoutAction(IServiceProvider services)
	: BaseAction<RefreshTokenParameter, Unit>(services)
{
	protected override async Task<Unit> Action(RefreshTokenParameter parameter)
	{
		IRefreshTokenHandler handler = Resolve<IRefreshTokenHandlerResolver>().Resolve();
		string? token = handler.ReadInboundToken(parameter);

		if (token is not null)
		{
			string? jti = JtiExtractor.Extract(token);
			await handler.RevokeAsync(token, jti);
		}

		return Unit.Default;
	}
}