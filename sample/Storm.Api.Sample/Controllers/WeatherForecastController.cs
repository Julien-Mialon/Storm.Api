using Microsoft.AspNetCore.Mvc;
using Storm.Api.Swaggers.Attributes;

namespace Storm.Api.Sample.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
	private static readonly string[] Summaries = new[] {"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"};

	private readonly ILogger<WeatherForecastController> _logger;

	public WeatherForecastController(ILogger<WeatherForecastController> logger)
	{
		_logger = logger;
	}

	[HttpGet]
	[Category("SampleCategory")]
	public IEnumerable<WeatherForecast> Get()
	{
		Random? rng = new Random();
		return Enumerable.Range(1, 5).Select(index => new WeatherForecast {Date = DateTime.Now.AddDays(index), TemperatureC = rng.Next(-20, 55), Summary = Summaries[rng.Next(Summaries.Length)]})
			.ToArray();
	}

	[HttpGet("all")]
	public IEnumerable<WeatherForecast> GetAll()
	{
		Random? rng = new Random();
		return Enumerable.Range(1, 5).Select(index => new WeatherForecast {Date = DateTime.Now.AddDays(index), TemperatureC = rng.Next(-20, 55), Summary = Summaries[rng.Next(Summaries.Length)]})
			.ToArray();
	}

	[HttpPost("file")]
	[Category("FileUpload")]
	public IEnumerable<WeatherForecast> UploadFile(IFormFile file)
	{
		Random? rng = new Random();
		return Enumerable.Range(1, 5).Select(index => new WeatherForecast {Date = DateTime.Now.AddDays(index), TemperatureC = rng.Next(-20, 55), Summary = Summaries[rng.Next(Summaries.Length)]})
			.ToArray();
	}

	[HttpGet("file")]
	[Category("FileUpload")]
	public IEnumerable<WeatherForecast> GetFile()
	{
		Random? rng = new Random();
		return Enumerable.Range(1, 5).Select(index => new WeatherForecast {Date = DateTime.Now.AddDays(index), TemperatureC = rng.Next(-20, 55), Summary = Summaries[rng.Next(Summaries.Length)]})
			.ToArray();
	}

	[HttpGet("file2")]
	[Category("FileUpload")]
	public IEnumerable<WeatherForecast> GetFile2()
	{
		Random? rng = new Random();
		return Enumerable.Range(1, 5).Select(index => new WeatherForecast {Date = DateTime.Now.AddDays(index), TemperatureC = rng.Next(-20, 55), Summary = Summaries[rng.Next(Summaries.Length)]})
			.ToArray();
	}
}