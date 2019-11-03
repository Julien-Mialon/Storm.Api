using System;
using Microsoft.Extensions.DependencyInjection;
using Storm.Api.Core.Services;

namespace Storm.Api.Core.Extensions
{
	public static class ServicesExtensions
	{
		public static DateTime CurrentDate(this IServiceProvider provider)
		{
			return provider.GetService<IDateService>().Now;
		}

		public static T Create<T>(this IServiceProvider provider)
		{
			return ActivatorUtilities.CreateInstance<T>(provider);
		}
	}
}