using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SQLiteRepository
{
    /// <summary>	Interface for a read only data repository. </summary>
    /// <typeparam name="TEntity">	Type of the entity. </typeparam>
    /// <typeparam name="TKey">   	Type of the key. </typeparam>
    public interface IReadWriteRepository<TEntity, in TKey> : IRepository
            where TEntity : class, IEntity<TKey>, new()
    {
        /// <summary>	Gets an entity using the given identifier. </summary>
        /// <param name="id">	The Identifier to use. </param>
        /// <returns>	A TEntity. </returns>
        ConfiguredTaskAwaitable<TEntity> Get(TKey id);

        /// <summary>	Gets all entities using the given identifiers. </summary>
        /// <param name="ids">	The Identifier to use. </param>
        /// <returns>	A list of TEntity. </returns>
        ConfiguredTaskAwaitable<List<TEntity>> Get(IEnumerable<TKey> ids);

        /// <summary>	Gets first item in this collection matching a given filter asynchronously. </summary>
        /// <param name="filter">	A linq expression to filter the results. </param>
        /// <returns>	A list of TEntity. </returns>
        ConfiguredTaskAwaitable<TEntity> Get<TProperty>(Expression<Func<TEntity, bool>> filter);

        /// <summary>	Gets all items in this collection asynchronously. </summary>
        /// <param name="filter">	A linq expression to filter the results. </param>
		/// <param name="sorting">	A linq expression to sort the results.</param>
        /// <param name="page">	The requested page number. </param>
        /// <param name="pageSize">	The number of items per page.</param>
        /// <returns>
        ///     An list that allows foreach to be used to process all items in this collection.
        /// </returns>
		ConfiguredTaskAwaitable<List<TEntity>> GetAll<TProperty>(Expression<Func<TEntity, bool>>? filter, Expression<Func<TEntity, TProperty>>? sorting, int? page = null, int? pageSize = null);

        /// <summary>	Gets all items in this collection asynchronously. </summary>
        /// <param name="filter">	A linq expression to filter the results. </param>
        /// <param name="sorting">	A linq expression to sort the results.</param>
        /// <param name="page">	The requested page number. </param>
        /// <param name="pageSize">	The number of items per page.</param>
        /// <returns>
        ///     An list that allows foreach to be used to process all items in this collection.
        /// </returns>
        ConfiguredTaskAwaitable<int> Count(Expression<Func<TEntity, bool>>? filter);


        /// <summary>	Adds entity asynchronously. </summary>
        /// <param name="entity">	The entity to add. </param>
        /// <returns>	The number of rows added to the table. </returns>
        ConfiguredTaskAwaitable<int> Add(TEntity entity);

        /// <summary>	Adds a range asynchronously. </summary>
        /// <param name="entities">	An IEnumerable&lt;TEntity&gt; of items to append to this. </param>
        /// <returns>	The number of rows added to the table. </returns>
        ConfiguredTaskAwaitable<int> AddRange(IEnumerable<TEntity> entities);

        /// <summary>	Updates the given entity asynchronously. </summary>
        /// <param name="entity">	The entity to update. </param>
        /// <returns>	The number of rows updated. </returns>
        ConfiguredTaskAwaitable<int> Update(TEntity entity);

        /// <summary>	Updates the given entities asynchronously. </summary>
        /// <param name="entities">	The entities to update. </param>
        /// <returns>	The number of rows updated. </returns>
        ConfiguredTaskAwaitable<int> Update(IEnumerable<TEntity> entities);

        /// <summary>	Deletes the given ID asynchronously. </summary>
        /// <param name="id">	The Identifier to delete. </param>
        /// <returns>	The number of objects deleted. </returns>
        ConfiguredTaskAwaitable<int> Delete(TKey id);

        /// <summary>	Deletes the given entity asynchronously. </summary>
        /// <param name="entity">	The entity to delete. </param>
        /// <returns>	The number of objects deleted. </returns>
        ConfiguredTaskAwaitable<int> Delete(TEntity entity);

        /// <summary>	WARNING: Deletes every entity from the given table. </summary>
        /// <returns>	The number of objects deleted. </returns>
        ConfiguredTaskAwaitable<int> ClearTable();
    }
}
