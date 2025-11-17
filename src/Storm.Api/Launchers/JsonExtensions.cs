using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Storm.Api.Jsons;

namespace Storm.Api.Launchers;

public static class JsonExtensions
{
	public static IMvcBuilder AddJsonLibrary(this IMvcBuilder builder)
	{
		if (DefaultLauncherOptions.UseNewtonsoftJson)
		{
			return builder.AddNewtonsoftJson(options =>
			{
				options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
				options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
				options.SerializerSettings.ContractResolver = new DefaultContractResolver()
				{
					NamingStrategy = new DefaultNamingStrategy()
				};
			});
		}

		return builder.AddJsonOptions(options =>
		{
			options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
			options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
			options.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Disallow;
		});
	}
}