using SQLite;
using SQLiteRepository;
using System;

namespace Sample
{
    public class WeatherForecast : IEntity<int>
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string Summary { get; set; }
    }
}
