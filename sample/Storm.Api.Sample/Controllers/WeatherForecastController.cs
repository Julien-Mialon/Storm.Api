using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Storm.Api.Swaggers.Attributes;

namespace Storm.Api.Sample.Controllers
{
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
			var rng = new Random();
			return Enumerable.Range(1, 5).Select(index => new WeatherForecast {Date = DateTime.Now.AddDays(index), TemperatureC = rng.Next(-20, 55), Summary = Summaries[rng.Next(Summaries.Length)]})
				.ToArray();
		}

		[HttpGet("all")]
		public IEnumerable<WeatherForecast> GetAll()
		{
			var rng = new Random();
			return Enumerable.Range(1, 5).Select(index => new WeatherForecast {Date = DateTime.Now.AddDays(index), TemperatureC = rng.Next(-20, 55), Summary = Summaries[rng.Next(Summaries.Length)]})
				.ToArray();
		}

		[HttpPost("file")]
		[Category("FileUpload")]
		public IEnumerable<WeatherForecast> UploadFile(IFormFile file)
		{
			var rng = new Random();
			return Enumerable.Range(1, 5).Select(index => new WeatherForecast {Date = DateTime.Now.AddDays(index), TemperatureC = rng.Next(-20, 55), Summary = Summaries[rng.Next(Summaries.Length)]})
				.ToArray();
		}

		[HttpGet("file")]
		[Category("FileUpload")]
		public IEnumerable<WeatherForecast> GetFile()
		{
			var rng = new Random();
			return Enumerable.Range(1, 5).Select(index => new WeatherForecast {Date = DateTime.Now.AddDays(index), TemperatureC = rng.Next(-20, 55), Summary = Summaries[rng.Next(Summaries.Length)]})
				.ToArray();
		}

		[HttpGet("file2")]
		[Category("FileUpload")]
		public IEnumerable<WeatherForecast> GetFile2()
		{
			var rng = new Random();
			return Enumerable.Range(1, 5).Select(index => new WeatherForecast {Date = DateTime.Now.AddDays(index), TemperatureC = rng.Next(-20, 55), Summary = Summaries[rng.Next(Summaries.Length)]})
				.ToArray();
		}
	}
}