using Storm.Api.Authentications.Refresh.Storage;
using Storm.Api.Authentications.Refresh.Transport;
using Storm.Api.CQRS;

namespace Storm.Api.Authentications.Refresh;

public abstract class BaseLogoutAction<TParameter>(IServiceProvider services) : BaseAction<TParameter, Unit>(services)
	where TParameter : IRefreshTokenParameterConvertible
{
	protected override async Task<Unit> Action(TParameter parameter)
	{
		IRefreshTokenStorage storage = Resolve<IRefreshTokenStorage>();
		IRefreshTokenTransport transport = Resolve<IRefreshTokenTransportResolver>().Resolve();

		string? token = transport.ReadToken(parameter.AsRefreshTokenParameter());
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