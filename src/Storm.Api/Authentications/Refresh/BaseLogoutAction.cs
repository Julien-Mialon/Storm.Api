using Storm.Api.Authentications.Refresh.Storage;
using Storm.Api.Authentications.Refresh.Transport;
using Storm.Api.CQRS;

namespace Storm.Api.Authentications.Refresh;

public class BaseLogoutAction(IServiceProvider services)
	: BaseAction<RefreshTokenParameter, Unit>(services)
{
	protected override async Task<Unit> Action(RefreshTokenParameter parameter)
	{
		IRefreshTokenStorage storage = Resolve<IRefreshTokenStorage>();
		IRefreshTokenTransport transport = Resolve<IRefreshTokenTransportResolver>().Resolve();

		string? token = transport.ReadToken(parameter);
		if (token is not null)
		{
			string? jti = JtiExtractor.Extract(token);
			if (jti is not null)
			{
				await storage.RevokeAsync(jti);
			}

			transport.ClearToken();
		}

		return Unit.Default;
	}
}
