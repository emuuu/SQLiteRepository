using Microsoft.Extensions.Options;
using SQLite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace SQLiteRepository
{
	/// <summary>	A sqlite read/write repository. </summary>
	/// <typeparam name="TEntity">	Type of the entity. </typeparam>
	/// <typeparam name="TKey">   	Type of the key. </typeparam>
	public abstract class ReadWriteRepository<TEntity, TKey> : IReadWriteRepository<TEntity, TKey>
		where TEntity : class, IEntity<TKey>, new()
	{
        /// <summary>   Constructor. </summary>
        /// <param name="sqlLiteOptions">   The sqlite connection options. </param>
        protected ReadWriteRepository(IOptions<SQLiteDbOptions> sqliteOptions)
		{
			var context = new EntityContext<TEntity>(sqliteOptions);
            Connection = context.Database;
            Table = Connection.Table<TEntity>();
		}

        /// <summary>   Gets the sqlite table. </summary>
        /// <value> The entities table. </value>
        public virtual AsyncTableQuery<TEntity> Table { get; }

        /// <summary>   Gets the sqlite connection. </summary>
        /// <value> The entities database connection. </value>
        public virtual SQLiteAsyncConnection Connection { get; }


		/// <summary>	Gets a t entity using the given identifier asynchronously. </summary>
		/// <param name="id">	The Identifier to get. </param>
		/// <returns>	A TEntity. </returns>
		public virtual ConfiguredTaskAwaitable<TEntity> Get(TKey id)
        {
            return Table.FirstOrDefaultAsync(x => x.Id.Equals(id)).ConfigureAwait(false);
		}

        /// <summary>	Gets all entities using the given identifiers. WARNING: May be subjected to sql injections </summary>
        /// <param name="id">	The Identifier to get. </param>
        /// <returns>	A list of TEntity. </returns>
        public virtual ConfiguredTaskAwaitable<List<TEntity>> Get(IEnumerable<TKey> ids)
		{
            var parameter = $"({string.Join(',', ids.Select(x => x.ToString().Replace(";", "").Replace("'", "").Replace("--", "")))})";
            return Connection.QueryAsync<TEntity>($"SELECT * FROM [{typeof(TEntity).Name}] WHERE [Id] IN {parameter}").ConfigureAwait(false);
        }

        /// <summary>	Gets first item in this collection matching a given filter asynchronously. </summary>
        /// <param name="filter">	A linq expression to filter the results. </param>
        /// <returns>	A list of TEntity. </returns>
        public virtual ConfiguredTaskAwaitable<TEntity> Get<TProperty>(Expression<Func<TEntity, bool>> filter)
        {
            return Table
                .FirstOrDefaultAsync(filter)
				.ConfigureAwait(false);
        }


        /// <summary>	Gets all items in this collection asynchronously. </summary>
        /// <param name="filter">	A linq expression to filter the results. </param>
        /// <param name="sorting">	A linq expression to sort the results. </param>
        /// <param name="page">	The requested page number. </param>
        /// <param name="pageSize">	The number of items per page.</param>
        /// <returns>
        ///     An list that allows foreach to be used to process all items in this collection.
        /// </returns>
        public virtual ConfiguredTaskAwaitable<List<TEntity>> GetAll<TProperty>(Expression<Func<TEntity, bool>>? filter, Expression<Func<TEntity, TProperty>>? sorting, int? page = null, int? pageSize = null)
        {
            if (page.HasValue && pageSize.HasValue)
            {
                if (page < 1)
                {
                    page = 1;
                }
                if (pageSize < 1)
                {
                    pageSize = 1;
                }
            }
            if (sorting == null)
            {
                return Table
                    .Where(filter ?? (x => true))
                    .Skip(page.HasValue ? (page.Value - 1) * pageSize.Value : 0)
                    .Take(pageSize.HasValue ? pageSize.Value : int.MaxValue)
                    .ToListAsync().ConfigureAwait(false);
            }
            else
            {
                return Table
                    .Where(filter ?? (x => true))
                    .OrderBy(sorting)
                    .Skip(page.HasValue ? (page.Value - 1) * pageSize.Value : 0)
                    .Take(pageSize.HasValue ? pageSize.Value : int.MaxValue)
                    .ToListAsync().ConfigureAwait(false);
            }
        }

        /// <summary>	Gets all items in this collection asynchronously. </summary>
        /// <returns>
        ///     An list that allows foreach to be used to process all items in this collection.
        /// </returns>
        public virtual ConfiguredTaskAwaitable<List<TEntity>> GetAll()
        {
            return Table.ToListAsync().ConfigureAwait(false);
        }

        /// <summary>	Counts all items in this table asynchronously. </summary>
        /// <param name="filter">	A linq expression to filter the results. </param>
        /// <returns>
        ///     An list that allows foreach to be used to process all items in this collection.
        /// </returns>
        public virtual ConfiguredTaskAwaitable<int> Count(Expression<Func<TEntity, bool>>? filter)
		{
            return Table
                .Where(filter ?? (x => true))
				.CountAsync()
				.ConfigureAwait(false);
        }

        /// <summary>	Avoids leading or trailing whitespaces in string values. </summary>
        /// <param name="entity">	The entity to trim. </param>
        /// <returns>	A TEntity. </returns>
        private static TEntity TrimStrings(TEntity entity)
        {
            foreach (var property in typeof(TEntity).GetProperties())
            {
                var ignoreAttribute = (IgnoreAttribute[])property.GetCustomAttributes(typeof(IgnoreAttribute), false);
                if (ignoreAttribute.Length > 0)
                    continue;

                if (property.CanRead && property.CanWrite && property.PropertyType == typeof(string))
                {
                    var value = (string)property.GetValue(entity);
                    if (!string.IsNullOrWhiteSpace(value))
                        property.SetValue(entity, value.Trim());
                }
            }
            return entity;
        }

        /// <summary>	Adds entity asynchronously. </summary>
        /// <param name="entity">	The entity to add. </param>
        /// <returns>	The number of rows added to the table. </returns>
        public virtual ConfiguredTaskAwaitable<int> Add(TEntity entity)
        {
            return Connection.InsertAsync(entity).ConfigureAwait(false);
        }

        /// <summary>	Adds a range asynchronously. </summary>
        /// <param name="entities">	An IEnumerable&lt;TEntity&gt; of items to append to this. </param>
        /// <returns>	The number of rows added to the table. </returns>
        public virtual ConfiguredTaskAwaitable<int> AddRange(IEnumerable<TEntity> entities)
        {
            foreach (var property in typeof(TEntity).GetProperties())
            {
                if (property.PropertyType == typeof(string))
                {
                    foreach (var entity in entities)
                    {
                        var value = (string)property.GetValue(entity);
                        if (!string.IsNullOrWhiteSpace(value))
                            property.SetValue(entity, value.Trim());
                    }
                }
            }

            return Connection.InsertAllAsync(entities).ConfigureAwait(false);
        }

        /// <summary>	Updates the given entity asynchronously. </summary>
        /// <param name="entity">	The entity to update. </param>
        /// <returns>	The number of rows updated. </returns>
        public virtual ConfiguredTaskAwaitable<int> Update(TEntity entity)
        {
            entity = TrimStrings(entity);
            return Connection.UpdateAsync(entity).ConfigureAwait(false);
        }

        /// <summary>	Updates the given entities asynchronously. </summary>
        /// <param name="entities">	The entities to update. </param>
        /// <returns>	The number of rows updated. </returns>
        public virtual ConfiguredTaskAwaitable<int> Update(IEnumerable<TEntity> entities)
        {
            foreach (var property in typeof(TEntity).GetProperties())
            {
                if (property.PropertyType == typeof(string))
                {
                    foreach (var entity in entities)
                    {
                        var value = (string)property.GetValue(entity);
                        if (!string.IsNullOrWhiteSpace(value))
                            property.SetValue(entity, value.Trim());
                    }
                }
            }
            return Connection.UpdateAllAsync(entities).ConfigureAwait(false);
        }

        /// <summary>	Deletes the given ID asynchronously. </summary>
        /// <param name="id">	The Identifier to delete. </param>
        /// <returns>	The number of objects deleted. </returns>
        public virtual ConfiguredTaskAwaitable<int> Delete(TKey id)
        {
            return Connection.DeleteAsync<TEntity>(id).ConfigureAwait(false);
        }

        /// <summary>	Deletes the given entity asynchronously. </summary>
        /// <param name="entity">	The entity to delete. </param>
        /// <returns>	The number of objects deleted. </returns>
        public virtual ConfiguredTaskAwaitable<int> Delete(TEntity entity)
        {
            return Connection.DeleteAsync(entity).ConfigureAwait(false);
        }

        /// <summary>	Deletes the given IDs asynchronously. WARNING: May be subjected to sql injections. </summary>
        /// <param name="ids">	The Identifiers to delete. </param>
        /// <returns>	The number of objects deleted. </returns>
        public virtual ConfiguredTaskAwaitable<int> Delete(IEnumerable<TKey> ids)
        {
            var parameter = $"({string.Join(',', ids.Select(x => x.ToString().Replace(";", "").Replace("'", "").Replace("--", "")))})";
            return Connection.ExecuteAsync($"DELETE FROM [{typeof(TEntity).Name}] WHERE [Id] IN {parameter}").ConfigureAwait(false);
        }

        /// <summary>	Deletes the given entities asynchronously. WARNING: May be subjected to sql injections. </summary>
        /// <param name="entities">	The entities to delete. </param>
        /// <returns>	The number of objects deleted. </returns>
        public virtual ConfiguredTaskAwaitable<int> Delete(IEnumerable<TEntity> entities)
        {
            return Delete(entities.Select(x => x.Id));
        }

        /// <summary>	WARNING: Deletes every entity from the given table. </summary>
        /// <returns>	The number of objects deleted. </returns>
        public virtual ConfiguredTaskAwaitable<int> ClearTable()
        {
            return Connection.DeleteAllAsync<TEntity>().ConfigureAwait(false);
        }
    }
}
