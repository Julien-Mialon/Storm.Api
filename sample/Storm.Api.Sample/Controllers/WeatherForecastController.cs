using Microsoft.AspNetCore.Mvc;

namespace Storm.Api.Sample.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
	private static readonly string[] SUMMARIES = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

	private readonly ILogger<WeatherForecastController> _logger;
	private readonly TimeProvider _timeProvider;

	public WeatherForecastController(ILogger<WeatherForecastController> logger, TimeProvider timeProvider)
	{
		_logger = logger;
		_timeProvider = timeProvider;
	}

	[HttpGet]
	// [Category("SampleCategory")]
	public IEnumerable<WeatherForecast> Get()
	{
		Random? rng = new();
		return Enumerable.Range(1, 5)
			.Select(index => new WeatherForecast
			{
				Date = _timeProvider.GetLocalNow().DateTime.AddDays(index),
				TemperatureC = rng.Next(-20, 55),
				Summary = SUMMARIES[rng.Next(SUMMARIES.Length)],
			})
			.ToArray();
	}

	[HttpGet("all")]
	public IEnumerable<WeatherForecast> GetAll()
	{
		Random? rng = new();
		return Enumerable.Range(1, 5)
			.Select(index => new WeatherForecast
			{
				Date = _timeProvider.GetLocalNow().DateTime.AddDays(index),
				TemperatureC = rng.Next(-20, 55),
				Summary = SUMMARIES[rng.Next(SUMMARIES.Length)],
			})
			.ToArray();
	}

	[HttpPost("file")]
	// [Category("FileUpload")]
	public IEnumerable<WeatherForecast> UploadFile(IFormFile file)
	{
		Random? rng = new();
		return Enumerable.Range(1, 5)
			.Select(index => new WeatherForecast
			{
				Date = _timeProvider.GetLocalNow().DateTime.AddDays(index),
				TemperatureC = rng.Next(-20, 55),
				Summary = SUMMARIES[rng.Next(SUMMARIES.Length)],
			})
			.ToArray();
	}

	[HttpGet("file")]
	// [Category("FileUpload")]
	public IEnumerable<WeatherForecast> GetFile()
	{
		Random? rng = new();
		return Enumerable.Range(1, 5)
			.Select(index => new WeatherForecast
			{
				Date = _timeProvider.GetLocalNow().DateTime.AddDays(index),
				TemperatureC = rng.Next(-20, 55),
				Summary = SUMMARIES[rng.Next(SUMMARIES.Length)],
			})
			.ToArray();
	}

	[HttpGet("file2")]
	// [Category("FileUpload")]
	public IEnumerable<WeatherForecast> GetFile2()
	{
		Random? rng = new();
		return Enumerable.Range(1, 5)
			.Select(index => new WeatherForecast
			{
				Date = _timeProvider.GetLocalNow().DateTime.AddDays(index),
				TemperatureC = rng.Next(-20, 55),
				Summary = SUMMARIES[rng.Next(SUMMARIES.Length)],
			})
			.ToArray();
	}
}