# SqliteRepository
Straightforward CRUD repository for Sqlite

The main purpose of SqliteRepository is to provide an as easy as possible DAL for services using Sqlite without tinkering.

## How to use it
Add your connection string to [appsettings.json](https://github.com/emuuu/SqliteRepository/blob/master/sample/appsettings.json)
```
{
  "SQLiteDbOptions": {
    "DatabaseFilename": "YourDatabaseName.db3",
    "DatabaseLocation": "C:/your/database/directory"
  }
}
```
Then add an [entity](https://github.com/emuuu/SqliteRepository/blob/master/sample/WeatherForecast.cs) which implements IEntity<int> which basically means it has an ID field with PrimaryKey and AutoIncrement attributes.
```
    public class WeatherForecast : IEntity<int>
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        //...
    }
```
For each entity you need a [repository](https://github.com/emuuu/SqliteRepository/blob/master/sample/Repositories/IWeatherForecastRepository.cs) which [implements](https://github.com/emuuu/SqliteRepository/blob/master/sample/Repositories/WeatherForecastSqliteRepository.cs) IReadWriteRepository<TEntity, int>. It is possible to add your own methods to the repository and of course to override the default ones.
```
    public interface IWeatherForecastRepository : IReadWriteRepository<WeatherForecast, int>
    {
      Task<IList<WeatherForecast>> GetAllWeatherForecastsWith16Degree();
    }
```
```
    public class WeatherForecastSqliteRepository : ReadWriteRepository<WeatherForecast, int>, IWeatherForecastRepository
    {
        public WeatherForecastSqliteRepository(IOptions<SQLiteDbOptions> sqlLiteOptions) : base(sqlLiteOptions)
        {

        }
        
        public async Task<IList<WeatherForecast>> GetAllWeatherForecastsWith16Degree()
        {
            return await Table
              .Where(x=> x.TemperatureC == 16)
              .ToListAsync();
        }
    }
```
Last thing to do is to [inject](https://github.com/emuuu/SqliteRepository/blob/master/sample/Startup.cs) your connectionstring and the repository
```
  services.Configure<SQLiteDbOptions>(Configuration.GetSection("SQLiteDbOptions"));
  services.AddScoped<IWeatherForecastRepository, WeatherForecastRepository>();
```
