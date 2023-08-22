using Microsoft.Extensions.Options;
using SQLiteRepository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sample.Repositories
{
    public class WeatherForecastSqliteRepository : ReadWriteRepository<WeatherForecast, int>, IWeatherForecastRepository
    {
        public WeatherForecastSqliteRepository(IOptions<SQLiteDbOptions> sqlLiteOptions) : base(sqlLiteOptions)
        {

        }

        public async Task<IList<WeatherForecast>> GetAllWeatherForecastsWith16Degree()
        {
            return await Table
              .Where(x => x.TemperatureC == 16)
              .ToListAsync();
        }
    }
}
