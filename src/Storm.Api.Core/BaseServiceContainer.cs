using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace Storm.Api.Core
{
	public class BaseServiceContainer
	{
		protected IServiceProvider Services { get; }

		public BaseServiceContainer(IServiceProvider services)
		{
			Services = services;
		}

		protected TService Resolve<TService>([CallerFilePath] string file = null, [CallerLineNumber] int line = 0, [CallerMemberName] string member = null)
			where TService : class
		{
			TService service = Services.GetService<TService>();

			if (service is null)
			{
				throw new InvalidOperationException($"Can not resolve instance of service of type {typeof(TService).Name} from {member} in {file}:{line}");
			}

			return service;
		}
	}
}