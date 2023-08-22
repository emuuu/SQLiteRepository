using SQLiteRepository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sample.Repositories
{
    public interface IWeatherForecastRepository : IReadWriteRepository<WeatherForecast, int>
    {
        Task<IList<WeatherForecast>> GetAllWeatherForecastsWith16Degree();
    }
}
