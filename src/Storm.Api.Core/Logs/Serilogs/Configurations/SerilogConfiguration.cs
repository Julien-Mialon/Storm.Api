using Serilog;
using Storm.Api.Core.Logs.Serilogs.Senders;

namespace Storm.Api.Core.Logs.Serilogs.Configurations
{
	public class SerilogConfiguration
	{
		internal string LogFileName { get; set; } = "output.log";

		internal bool EnableConsoleLogging { get; set; } = false;
		internal LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;

		public ILogService CreateService()
		{
			return new LogService(CreateSender, MinimumLogLevel);
		}

		private ILogSender CreateSender(ILogService logService)
		{
			var logger = new LoggerConfiguration()
				.WriteTo.File("output.log", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
				.WriteTo.Console()
				.CreateLogger();

			return new SerilogSender(logger);
		}

		public static ISerilogConfigurationBuilder CreateBuilder() => new SerilogConfigurationBuilder();
	}
}