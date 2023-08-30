using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sample.Repositories;
using Newtonsoft.Json;

namespace Sample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IWeatherForecastRepository _weatherRepository;
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(IWeatherForecastRepository weatherRepository, ILogger<WeatherForecastController> logger)
        {
            _weatherRepository = weatherRepository;
            _logger = logger;
        }

        [HttpGet(Name = nameof(GetAllWeatherForecasts))]
        public async Task<IActionResult> GetAllWeatherForecasts([FromQuery] IEnumerable<int>? weatherForecastIDs = null, [FromQuery] int? page = null, [FromQuery] int? pageSize = null)
        {
            if(weatherForecastIDs!= null)
            {
                return Ok(await _weatherRepository.Get(weatherForecastIDs));
            }

            if (!page.HasValue)
                page = 1;

            if (!pageSize.HasValue)
                pageSize = 10;

            var allItemCount = await _weatherRepository.Count(null);
            var paginationMetadata = new
            {
                totalCount = allItemCount,
                pageSize = pageSize,
                currentPage = page,
                totalPages = (int)Math.Ceiling(allItemCount / (double)pageSize)
            };

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));


            return Ok(await _weatherRepository.GetAll(x => true, x => x.TemperatureC, page, pageSize));
        }


        [HttpGet]
        [Route("degrees/{degree:int}", Name = nameof(GetWeatherForecastsByDegreeOrderedByDate))]
        public async Task<IActionResult> GetWeatherForecastsByDegreeOrderedByDate(int degree)
        {
            return Ok(await _weatherRepository.GetAll(x => x.TemperatureC == degree, x => x.Date));
        }


        [HttpGet]
        [Route("degrees/16", Name = nameof(GetAllWeatherForecastsWith16Degree))]
        public async Task<IActionResult> GetAllWeatherForecastsWith16Degree()
        {
            return Ok(await _weatherRepository.GetAllWeatherForecastsWith16Degree());
        }

        [HttpPost(Name = nameof(CreateWeatherForecast))]
        public async Task<IActionResult> CreateWeatherForecast([FromBody] WeatherForecast weatherForecast)
        {
            await _weatherRepository.Add(weatherForecast);
            return CreatedAtRoute(nameof(GetSingleWeatherForecast), new { weatherForecastID = weatherForecast.Id }, weatherForecast);
        }

        [HttpGet("{weatherForecastID}", Name = nameof(GetSingleWeatherForecast))]
        public async Task<IActionResult> GetSingleWeatherForecast(int weatherForecastID)
        {
            return Ok(await _weatherRepository.Get(weatherForecastID));
        }

        [HttpPut("{weatherForecastID}", Name = nameof(UpdateWeatherForecast))]
        public async Task<IActionResult> UpdateWeatherForecast(int weatherForecastID, [FromBody] WeatherForecast weatherForecast)
        {
            weatherForecast.Id = weatherForecastID;
            return Ok(await _weatherRepository.Update(weatherForecast));
        }

        [HttpDelete("{weatherForecastID}", Name = nameof(DeleteWeatherForecast))]
        public async Task<IActionResult> DeleteWeatherForecast(int weatherForecastID)
        {
            await _weatherRepository.Delete(weatherForecastID);
            return Ok();
        }



        [HttpDelete(Name = nameof(DeleteWeatherForecasts))]
        public async Task<IActionResult> DeleteWeatherForecasts([FromQuery] IEnumerable<int>? weatherForecastIDs = null)
        {
            if (weatherForecastIDs == null)
            {
                await _weatherRepository.ClearTable();
            }
            else
            {
                await _weatherRepository.Delete(weatherForecastIDs);
            }

            return Ok();
        }

    }
}
