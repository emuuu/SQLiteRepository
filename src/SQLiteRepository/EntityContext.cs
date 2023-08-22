using Microsoft.Extensions.Options;
using SQLite;
using System;
using System.IO;

namespace SQLiteRepository
{
    /// <summary>	A sqlite context for specified entity. </summary>
    /// <typeparam name="TEntity">	Type of the entity. </typeparam>
    public class EntityContext<TEntity> where TEntity : new()
	{
        /// <summary>   Constructor. </summary>
        /// <param name="sqliteOptions">   The sqlite connection options. </param>
        public EntityContext(IOptions<SQLiteDbOptions> sqliteOptions)
		{
            Database = new SQLiteAsyncConnection(Path.Combine(sqliteOptions.Value.DatabaseLocation, sqliteOptions.Value.DatabaseFilename), sqliteOptions.Value.Flags);
            var createTableTask = Database.CreateTableAsync<TEntity>();
            createTableTask.Wait();
        }

        /// <summary>   Gets the sqlite read/write database connection. </summary>
        /// <value> The sqlite read/write database connection. </value>
        public readonly SQLiteAsyncConnection Database = null;
	}
}
