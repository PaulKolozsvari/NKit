﻿//namespace NKit.Data.DB.LINQ
//{
//    #region Using Directives

//    using System;
//    using System.Collections.Generic;
//    using System.ComponentModel.DataAnnotations;
//    using System.ComponentModel.DataAnnotations.Schema;
//    using System.Linq;
//    using System.Linq.Expressions;
//    using System.Reflection;
//    using System.Threading.Tasks;
//    using Microsoft.EntityFrameworkCore;
//    using NKit.Data;
//    using NKit.Standard.Data.DB.LINQ;
//    using Dapper;
//    using System.Data.Common;
//    using Microsoft.Extensions.Options;
//    using NKit.Utilities;
//    using Microsoft.Extensions.Logging;
//    using NKit.Data.DB.LINQ.Models;
//    using System.Data;
//    using NKit.Settings.Default;

//    #endregion //Using Directives

//    /// <summary>
//    /// A facade wrapper (repository) around an Entity Framework DbContext used to manage it to provide CRUD operations.
//    /// Methods in this class are NOT wrapped in Transactions.
//    /// Managing DbContext the right way with Entity Framework 6: an in-depth guide: https://mehdi.me/ambient-dbcontext-in-ef6/
//    /// </summary>
//    /// <typeparam name="D">The underlying entity framework DbContext which should have been registered as service..</typeparam>
//    public class NKitDbRespositoryBase
//    {
//        #region Constructors

//        /// <summary>
//        /// Creates a funnel context using the specified servicce provider from which it will get the entity framework DbContext.
//        /// </summary>
//        /// <param name="serviceProvider">ServiceProvider to be used to get the DbContext of type D</param>
//        /// <param name="dbContextType">The type of the DbContext that will be be created when using this constructor.</param>
//        /// <param name="generalOptions">General settings.</param>
//        /// <param name="dbContextOptions">DbContext related settings.</param>
//        /// <param name="loggingOptions">Logging related settings.</param>
//        public NKitDbRespositoryBase(
//            IServiceProvider serviceProvider, 
//            Type dbContextType,
//            IOptions<NKitGeneralSettings> generalOptions,
//            IOptions<NKitDbRepositorySettings> dbContextOptions, 
//            IOptions<NKitLoggingSettings> loggingOptions)
//        {
//            DataValidator.ValidateObjectNotNull(serviceProvider, nameof(serviceProvider), nameof(NKitDbRespositoryBase));
//            DataValidator.ValidateObjectNotNull(dbContextType, nameof(dbContextType), nameof(NKitDbRespositoryBase));
//            DataValidator.ValidateObjectNotNull(generalOptions, nameof(generalOptions), nameof(NKitDbRespositoryBase));
//            DataValidator.ValidateObjectNotNull(dbContextOptions, nameof(dbContextOptions), nameof(NKitDbRespositoryBase));
//            DataValidator.ValidateObjectNotNull(loggingOptions, nameof(loggingOptions), nameof(NKitDbRespositoryBase));
//            _generalSettings = generalOptions.Value;
//            _dbRepositorySettings = dbContextOptions.Value;
//            _loggingSettings = loggingOptions.Value;
//            _serviceProvider = serviceProvider;
//            _dbContextType = dbContextType;
//        }

//        /// <summary>
//        /// Creates a funnel context using the specified entity framework DbContext.
//        /// </summary>
//        /// <param name="db">The DbContext to use for running operations for the underlying database.</param>
//        /// <param name="generalOptions">General settings.</param>
//        /// <param name="dbContextOptions">DbContext related settings.</param>
//        /// <param name="loggingOptions">Logging related settings.</param>
//        public NKitDbRespositoryBase(
//            DbContext db,
//            IOptions<NKitGeneralSettings> generalOptions,
//            IOptions<NKitDbRepositorySettings> dbContextOptions, 
//            IOptions<NKitLoggingSettings> loggingOptions)
//        {
//            DataValidator.ValidateObjectNotNull(db, nameof(db), nameof(NKitDbRespositoryBase));
//            DataValidator.ValidateObjectNotNull(generalOptions, nameof(generalOptions), nameof(NKitDbRespositoryBase));
//            DataValidator.ValidateObjectNotNull(dbContextOptions, nameof(dbContextOptions), nameof(NKitDbRespositoryBase));
//            DataValidator.ValidateObjectNotNull(loggingOptions, nameof(loggingOptions), nameof(NKitDbRespositoryBase));
//            _generalSettings = generalOptions.Value;
//            _dbRepositorySettings = dbContextOptions.Value;
//            _loggingSettings = loggingOptions.Value;
//            _db = db;
//            _db.Database.SetConnectionString(_dbRepositorySettings.DatabaseConnectionString);
//            _db.Database.SetCommandTimeout(_dbRepositorySettings.DatabaseCommandTimeoutSeconds);
//        }

//        #endregion //Constructors

//        #region Fields

//        protected IServiceProvider _serviceProvider;
//        protected Type _dbContextType;
//        protected DbContext _db;
//        protected NKitGeneralSettings _generalSettings;
//        protected NKitDbRepositorySettings _dbRepositorySettings;
//        protected NKitLoggingSettings _loggingSettings;

//        #endregion //Fields

//        #region Properties

//        /// <summary>
//        /// Gets or sets the underlying entity framework DbContext which should have been registered as a service.
//        /// </summary>
//        public DbContext DB
//        {
//            get
//            {
//                if ((_db == null) && (_serviceProvider != null))
//                {
//                    _db = (DbContext)_serviceProvider.GetService(_dbContextType);
//                    _db.Database.SetConnectionString(_dbRepositorySettings.DatabaseConnectionString);
//                    _db.Database.SetCommandTimeout(_dbRepositorySettings.DatabaseCommandTimeoutSeconds);
//                }
//                return _db;
//            }
//            set
//            {
//                if (value == null)
//                {
//                    throw new NullReferenceException($"{nameof(DbContext)} may not be null when setting it on the {nameof(NKitDbRespositoryBase)}");
//                }
//                _db = value;
//            }
//        }

//        #endregion //Properties

//        #region Methods

//        /// <summary>
//        /// Executes raw SQL queries on the underlying DbContext.
//        /// https://stackoverflow.com/questions/35631903/raw-sql-query-without-dbset-entity-framework-core
//        /// </summary>
//        /// <typeparam name="T">The entity type to convert the data to.</typeparam>
//        /// <param name="query">The raw SQL query.</param>
//        /// <param name="map">Mappings between the data received from the DbDataReader and the specified entity type.</param>
//        /// <returns></returns>
//        public List<T> RawSqlQuery<T>(string query, Func<DbDataReader, T> map)
//        {
//            using (var command = DB.Database.GetDbConnection().CreateCommand())
//            {
//                command.CommandText = query;
//                command.CommandType = CommandType.Text;

//                DB.Database.OpenConnection();

//                using (DbDataReader dbDataReader = command.ExecuteReader())
//                {
//                    var entities = new List<T>();
//                    while (dbDataReader.Read())
//                    {
//                        entities.Add(map(dbDataReader));
//                    }
//                    return entities;
//                }
//            }
//        }

//        /// <summary>
//        /// Saves (updates/inserts) an entity to the table corrseponding to the entity type.
//        /// If the entity's surrogate key is an identity entity will be inserted and not updated.
//        /// </summary>
//        /// <typeparam name="E">The type of the entity i.e. which table it will be saved to.</typeparam>
//        /// <param name="entity">The the entity to save.</param>
//        /// <param name="entityIdentifier">The value unique identifiying the entity</param>
//        /// <returns>Returns a list of change results i.e. what entities where updated</returns>
//        public virtual List<LinqFunnelChangeResult> Save<E>(E entity, object entityIdentifier) where E : class
//        {
//            List<LinqFunnelChangeResult> result = SaveWithoutSavingToDb(typeof(E), entity, entityIdentifier);
//            DB.SaveChanges();
//            return result;
//        }

//        /// <summary>
//        /// Saves (updates/inserts) an entity to the table corrseponding to the entity type.
//        /// If the entity's surrogate key is an identity entity will be inserted and not updated.
//        /// </summary>
//        /// <typeparam name="E">The type of the entity i.e. which table it will be saved to.</typeparam>
//        /// <param name="entity">The the entity to save.</param>
//        /// <param name="entityIdentifier">The value unique identifiying the entity</param>
//        /// <returns>Returns a list of change results i.e. what entities where updated</returns>
//        public async virtual Task<List<LinqFunnelChangeResult>> SaveAsync<E>(E entity, object entityIdentifier) where E : class
//        {
//            List<LinqFunnelChangeResult> result = SaveWithoutSavingToDb(typeof(E), entity, entityIdentifier);
//            await DB.SaveChangesAsync();
//            return result;
//        }

//        /// <summary>
//        /// Saves (updates/inserts) an entity to the table corrseponding to the entity type.
//        /// If the entity's surrogate key is an identity entity will be inserted and not updated.
//        /// </summary>
//        /// <typeparam name="E">The type of the entity i.e. which table it will be saved to.</typeparam>
//        /// <param name="entityType">The Type of entity being inserted.</param>
//        /// <param name="entity">The the entity to save.</param>
//        /// <param name="entityIdentifier">The value unique identifiying the entity</param>
//        /// <returns>Returns a list of change results i.e. what entities where updated</returns>
//        protected virtual List<LinqFunnelChangeResult> SaveWithoutSavingToDb(Type entityType, object entity, object entityIdentifier)
//        {
//            PropertyInfo surrogateKey = GetEntitySurrogateKey(entityType);
//            bool containsIdentityColumn = IsIdentityColumn(surrogateKey);
//            object original = null;
//            object surrogateKeyValue = surrogateKey.GetValue(entity, null);
//            original = GetEntityBySurrogateKey(entityType, surrogateKeyValue);
//            List<LinqFunnelChangeResult> result = null;
//            if (original == null)
//            {
//                DB.Add(entity);
//                result = new List<LinqFunnelChangeResult>();
//                result.Add(new LinqFunnelChangeResult()
//                {
//                    SurrogateKey = surrogateKeyValue,
//                    EntityIdentifier = entityIdentifier,
//                    Function = "INSERT",
//                    DateChanged = DateTime.Now,
//                    EntityChanged = entityType.Name,
//                    FieldChanged = surrogateKey.Name,
//                    NewValue = surrogateKey.GetValue(entity, null)
//                });
//            }
//            else
//            {
//                result = UpdateOriginalEntity(entityType, original, entity, surrogateKeyValue, entityIdentifier);
//            }
//            return result;
//        }

//        /// <summary>
//        /// Saves (updates/inserts) an entity to the table corrseponding to the entity type.
//        /// If the entity's surrogate key is an identity entity will be inserted and not updated.
//        /// </summary>
//        /// <typeparam name="E">The type of the entity i.e. which table it will be saved to.</typeparam>
//        /// <param name="entityType">The Type of entity being inserted.</param>
//        /// <param name="entity">The the entity to save.</param>
//        /// <param name="entityIdentifier">The value unique identifiying the entity</param>
//        /// <returns>Returns a list of change results i.e. what entities where updated</returns>
//        public virtual List<LinqFunnelChangeResult> Save(Type entityType, object entity, object entityIdentifier)
//        {
//            List<LinqFunnelChangeResult> result = SaveWithoutSavingToDb(entityType, entity, entityIdentifier);
//            DB.SaveChanges();
//            return result;
//        }

//        /// <summary>
//        /// Saves (updates/inserts) an entity to the table corrseponding to the entity type.
//        /// If the entity's surrogate key is an identity entity will be inserted and not updated.
//        /// </summary>
//        /// <typeparam name="E">The type of the entity i.e. which table it will be saved to.</typeparam>
//        /// <param name="entityType">The Type of entity being inserted.</param>
//        /// <param name="entity">The the entity to save.</param>
//        /// <param name="entityIdentifier">The value unique identifiying the entity</param>
//        /// <returns>Returns a list of change results i.e. what entities where updated</returns>
//        public async virtual Task<List<LinqFunnelChangeResult>> SaveAsync(Type entityType, object entity, object entityIdentifier)
//        {
//            List<LinqFunnelChangeResult> result = SaveWithoutSavingToDb(entityType, entity, entityIdentifier);
//            await DB.SaveChangesAsync();
//            return result;
//        }

//        /// <summary>
//        /// Inserts an entity to the database context without saving to the database.
//        /// </summary>
//        /// <param name="entity">The entity being inserted.</param>
//        /// <param name="entityIdentifier">The value unique identifiying the entity</param>
//        /// <returns>A list of change results.</returns>
//        public virtual List<LinqFunnelChangeResult> Insert<E>(E entity, object entityIdentifier) where E : class
//        {
//            return Insert(typeof(E), entity, entityIdentifier);
//        }

//        /// <summary>
//        /// Inserts an entity to the database context without saving to the database.
//        /// </summary>
//        /// <param name="entity">The entity being inserted.</param>
//        /// <param name="entityIdentifier">The value uniquely identifiying the entity</param>
//        /// <returns>A list of change results.</returns>
//        public async virtual Task<List<LinqFunnelChangeResult>> InsertAsync<E>(E entity, object entityIdentifier) where E : class
//        {
//            Task<List<LinqFunnelChangeResult>> taskResult = InsertAsync(typeof(E), entity, entityIdentifier);
//            await taskResult;
//            return taskResult.Result;
//        }

//        /// <summary>
//        /// Inserts an entity to the database context without saving to the database.
//        /// </summary>
//        /// <param name="entityType">The Type of entity being inserted.</param>
//        /// <param name="entity">The entity being inserted.</param>
//        /// <param name="entityIdentifier">The value uniquely identifiying the entity</param>
//        /// <returns>A list of change results.</returns>
//        protected virtual List<LinqFunnelChangeResult> InsertWithoutSaveToDb(Type entityType, object entity, object entityIdentifier)
//        {
//            PropertyInfo surrogateKey = GetEntitySurrogateKey(entityType);
//            bool containsIdentityColumn = IsIdentityColumn(surrogateKey);
//            object surrogateKeyValue = surrogateKey.GetValue(entity, null);

//            DB.Add(entity);
//            List<LinqFunnelChangeResult> result = new List<LinqFunnelChangeResult>();
//            result.Add(new LinqFunnelChangeResult()
//            {
//                SurrogateKey = surrogateKeyValue,
//                EntityIdentifier = entityIdentifier,
//                Function = "INSERT",
//                DateChanged = DateTime.Now,
//                EntityChanged = entityType.Name,
//                FieldChanged = surrogateKey.Name,
//                NewValue = surrogateKey.GetValue(entity, null)
//            });
//            return result;
//        }

//        /// <summary>
//        /// Inserts an entity to the database context and calls save on the database.
//        /// </summary>
//        /// <param name="entityType">The Type of entity being inserted.</param>
//        /// <param name="entity">The entity being inserted.</param>
//        /// <param name="entityIdentifier">The value uniquely identifiying the entity</param>
//        /// <returns>A list of change results.</returns>
//        public virtual List<LinqFunnelChangeResult> Insert(Type entityType, object entity, object entityIdentifier)
//        {
//            List<LinqFunnelChangeResult> result = InsertWithoutSaveToDb(entityType, entity, entityIdentifier);
//            DB.SaveChanges();
//            return result;
//        }

//        /// <summary>
//        /// Inserts an entity to the database context and calls async save on the database.
//        /// </summary>
//        /// <param name="entityType">The Type of entity being inserted.</param>
//        /// <param name="entity">The entity being inserted.</param>
//        /// <param name="entityIdentifier">The value uniquely identifiying the entity</param>
//        /// <returns>A list of change results.</returns>
//        public async Task<List<LinqFunnelChangeResult>> InsertAsync(Type entityType, object entity, object entityIdentifier)
//        {
//            List<LinqFunnelChangeResult> result = InsertWithoutSaveToDb(entityType, entity, entityIdentifier);
//            await DB.SaveChangesAsync();
//            return result;
//        }

//        /// <summary>
//        /// Determines the primary key of an entity type. The first primary key found on the entity type i.e.
//        /// the assumption is made that the entity type only has one primary key, which is the surrogate key.
//        /// </summary>
//        /// <typeparam name="E">The entity type i.e. the table whose surrogate key needs to be determined.</typeparam>
//        /// <returns>Retruns the PropertyInfo corresponding to the column which is the surrogate key for the specified entity type.</returns>
//        public virtual PropertyInfo GetEntitySurrogateKey<E>()
//        {
//            return EntityReader.GetLinqToSqlEntitySurrogateKey<E>();
//        }

//        /// <summary>
//        /// Determines the primary key of an entity type. The first primary key found on the entity type i.e.
//        /// the assumption is made that the entity type only has one primary key, which is the surrogate key.
//        /// </summary>
//        /// <typeparam name="E">The entity type i.e. the table whose surrogate key needs to be determined.</typeparam>
//        /// <returns>Retruns the PropertyInfo corresponding to the column which is the surrogate key for the specified entity type.</returns>
//        public virtual PropertyInfo GetEntitySurrogateKey(Type entityType)
//        {
//            return EntityReader.GetLinqToSqlEntitySurrogateKey(entityType);
//        }

//        /// <summary>
//        /// Determines whether a property is an identity column.
//        /// </summary>
//        /// <param name="p"></param>
//        /// <returns>Returns true if the property is an identity column.</returns>
//        public virtual bool IsIdentityColumn(PropertyInfo p)
//        {
//            return EntityReader.IsLinqToSqlEntityPropertyIdentityColumn(p);
//        }

//        /// <summary>
//        /// Updates the original entity with values of the latest entities. In other words, it copies the
//        /// column values of the latest entity to the original entity.
//        /// </summary>
//        /// <typeparam name="E">The entity type i.e. the table whose original record will be updated.</typeparam>
//        /// <param name="original">The original entity retrieved from the database.</param>
//        /// <param name="latest">The latest entity received from the client.</param>
//        /// <param name="surrogateKeyValue">The value of the entity's surrogate key uniquely identifying the entity.</param>
//        /// <param name="entityIdentifier">The value unique identifiying the entity</param>
//        /// <returns>Returns a list of change results containing all the fields that were changed and their original and new values.</returns>
//        protected virtual List<LinqFunnelChangeResult> UpdateOriginalEntity<E>(E original, E latest, object surrogateKeyValue, object entityIdentifier) where E : class
//        {
//            return UpdateOriginalEntity(typeof(E), original, latest, surrogateKeyValue, entityIdentifier);
//        }

//        /// <summary>
//        /// Updates the original entity with values of the latest entities. In other words, it copies the
//        /// column values of the latest entity to the original entity.
//        /// </summary>
//        /// <typeparam name="E">The entity type i.e. the table whose original record will be updated.</typeparam>
//        /// <param name="entityType">The Type of entity being inserted.</param>
//        /// <param name="original">The original entity retrieved from the database.</param>
//        /// <param name="latest">The latest entity received from the client.</param>
//        /// <param name="surrogateKeyValue">The value of the entity's surrogate key uniquely identifying the entity.</param>
//        /// <param name="entityIdentifier">The value unique identifiying the entity</param>
//        /// <returns>Returns a list of change results containing all the fields that were changed and their original and new values.</returns>
//        protected virtual List<LinqFunnelChangeResult> UpdateOriginalEntity(
//            Type entityType,
//            object original,
//            object latest,
//            object surrogateKeyValue,
//            object entityIdentifier)
//        {
//            if (entityType != original.GetType())
//            {
//                throw new ArgumentException(string.Format(
//                    "Entity Type of {0} does not match the original entity's type of {1}.",
//                    entityType.FullName,
//                    original.GetType().FullName));
//            }
//            if (original.GetType() != latest.GetType())
//            {
//                throw new ArgumentException(string.Format(
//                    "Cannot update original of type {0} because latest entity is of type {1}.",
//                    original.GetType().FullName,
//                    latest.GetType().FullName));
//            }
//            List<LinqFunnelChangeResult> result = new List<LinqFunnelChangeResult>();
//            PropertyInfo[] properties = entityType.GetProperties();
//            foreach (PropertyInfo p in properties)
//            {
//                KeyAttribute keyAttribute = p.GetCustomAttribute<KeyAttribute>();
//                ForeignKeyAttribute foreignKeyAttribute = p.GetCustomAttribute<ForeignKeyAttribute>();
//                if (keyAttribute != null || foreignKeyAttribute != null)
//                {
//                    continue; //Property is either a key, or foreign key and therefore should not be saved/updated.
//                }
//                object originalValue = p.GetValue(original, null);
//                object latestValue = p.GetValue(latest, null);
//                if (object.Equals(originalValue, latestValue))
//                {
//                    continue;
//                }
//                result.Add(new LinqFunnelChangeResult()
//                {
//                    SurrogateKey = surrogateKeyValue,
//                    EntityIdentifier = entityIdentifier,
//                    Function = "UPDATE",
//                    DateChanged = DateTime.Now,
//                    EntityChanged = entityType.Name,
//                    FieldChanged = p.Name,
//                    OriginalValue = originalValue,
//                    NewValue = latestValue,
//                });
//                p.SetValue(original, latestValue, null);
//            }
//            return result;
//        }

//        /// <summary>
//        /// Deletes an entity from the table corresponding to the entity type.
//        /// </summary>
//        /// <typeparam name="E">The entity type i.e. which table it will be deleted from.</typeparam>
//        /// <param name="entity">The entity to be deleted.</param>
//        /// <param name="entityIdentifier">Unique value identifying the entity.</param>
//        /// <returns>Returns a list of change results.</returns>
//        public virtual List<LinqFunnelChangeResult> Delete<E>(E entity, object entityIdentifier) where E : class
//        {
//            List<LinqFunnelChangeResult> result = this.Delete<E, object>(entity, entityIdentifier, false);
//            return result;
//        }

//        /// <summary>
//        /// Deletes an entity from the table corresponding to the entity type.
//        /// </summary>
//        /// <typeparam name="E">The entity type i.e. which table it will be deleted from.</typeparam>
//        /// <param name="entity">The entity to be deleted.</param>
//        /// <param name="entityIdentifier">Unique value identifying the entity.</param>
//        /// <returns>Returns a list of change results.</returns>
//        public async virtual Task<List<LinqFunnelChangeResult>> DeleteAsync<E>(E entity, object entityIdentifier) where E : class
//        {
//            Task<List<LinqFunnelChangeResult>> taskResult = this.DeleteAsync<E, object>(entity, entityIdentifier, false);
//            await taskResult;
//            return taskResult.Result;
//        }

//        /// <summary>
//        /// Deletes an entity from the table corresponding to the entity type.
//        /// </summary>
//        /// <typeparam name="E">The entity type i.e. which table it will be deleted from.</typeparam>
//        /// <param name="entity">The entity to be deleted.</param>
//        /// <param name="entityIdentifier">Unique value identifying the entity.</param>
//        /// <returns>Returns a list of change results.</returns>
//        public virtual List<LinqFunnelChangeResult> Delete(object entity, object entityIdentifier)
//        {
//            List<LinqFunnelChangeResult> result = this.DeleteWithoutSavingToDb(entity, entityIdentifier, false, null);
//            DB.SaveChanges();
//            return result;
//        }

//        /// <summary>
//        /// Deletes an entity from the table corresponding to the entity type.
//        /// </summary>
//        /// <typeparam name="E">The entity type i.e. which table it will be deleted from.</typeparam>
//        /// <param name="entity">The entity to be deleted.</param>
//        /// <param name="entityIdentifier">Unique value identifying the entity.</param>
//        /// <returns>Returns a list of change results.</returns>
//        public async virtual Task<List<LinqFunnelChangeResult>> DeleteAsync(object entity, object entityIdentifier)
//        {
//            List<LinqFunnelChangeResult> result = this.DeleteWithoutSavingToDb(entity, entityIdentifier, false, null);
//            await DB.SaveChangesAsync();
//            return result;
//        }

//        /// <summary>
//        /// Deletes an entity from the table correspoding to the entity type (E) and creates
//        /// a tombstone in the table correspoging to the tombstone entity type (T) if the 
//        /// createTombstone flag is set to true.
//        /// </summary>
//        /// <typeparam name="E">The entity type of the entity which will be deleted i.e. the table from where it will be deleted.</typeparam>
//        /// <typeparam name="T">The tombstone entity type i.e. the table where an tombstone will be created.</typeparam>
//        /// <param name="entity">The entity to be deleted</param>
//        /// <param name="entityIdentifier">Unique value identifying the entity.</param>
//        /// <param name="createTombstone">Indicates whether a tombstone should be created.</param>
//        /// <returns>Returns a list of change results.</returns>
//        public virtual List<LinqFunnelChangeResult> Delete<E, T>(E entity, object entityIdentifier, bool createTombstone)
//            where E : class
//            where T : class
//        {
//            List<LinqFunnelChangeResult> result = DeleteWithoutSavingToDb(entity, entityIdentifier, createTombstone, typeof(T));
//            DB.SaveChanges();
//            return result;
//        }

//        /// <summary>
//        /// Deletes an entity from the table correspoding to the entity type (E) and creates
//        /// a tombstone in the table correspoging to the tombstone entity type (T) if the 
//        /// createTombstone flag is set to true.
//        /// </summary>
//        /// <typeparam name="E">The entity type of the entity which will be deleted i.e. the table from where it will be deleted.</typeparam>
//        /// <typeparam name="T">The tombstone entity type i.e. the table where an tombstone will be created.</typeparam>
//        /// <param name="entity">The entity to be deleted</param>
//        /// <param name="entityIdentifier">Unique value identifying the entity.</param>
//        /// <param name="createTombstone">Indicates whether a tombstone should be created.</param>
//        /// <returns>Returns a list of change results.</returns>
//        public async virtual Task<List<LinqFunnelChangeResult>> DeleteAsync<E, T>(E entity, object entityIdentifier, bool createTombstone)
//            where E : class
//            where T : class
//        {
//            List<LinqFunnelChangeResult> result = DeleteWithoutSavingToDb(entity, entityIdentifier, createTombstone, typeof(T));
//            await DB.SaveChangesAsync();
//            return result;
//        }

//        /// <summary>
//        /// Deletes an entity from the table correspoding to the entity type (E) and creates
//        /// a tombstone in the table correspoging to the tombstone entity type (T) if the 
//        /// createTombstone flag is set to true.
//        /// </summary>
//        /// <param name="entity">The entity to be deleted</param>
//        /// <param name="entityIdentifier">Unique value identifying the entity.</param>
//        /// <param name="createTombstone">Indicates whether a tombstone should be created.</param>
//        /// <param name="tombstoneType">The tombstone entity type i.e. the table where an tombstone will be created.</param>
//        /// <returns>Returns a list of change results.</returns>
//        protected virtual List<LinqFunnelChangeResult> DeleteWithoutSavingToDb(object entity, object entityIdentifier, bool createTombstone, Type tombstoneType)
//        {
//            Type entityType = entity.GetType();
//            PropertyInfo surrogateKey = GetEntitySurrogateKey(entityType);
//            object surrogateKeyValue = surrogateKey.GetValue(entity, null);
//            object original = GetEntityBySurrogateKey(entityType, surrogateKeyValue, false);
//            if (original == null)
//            {
//                throw new Exception(
//                    string.Format("Could not find entity with key {0} and value {1} to delete.",
//                    GetEntitySurrogateKey(entityType).Name,
//                    GetEntitySurrogateKey(entityType).GetValue(entity, null)));
//            }
//            if (createTombstone)
//            {
//                object tombstone = Activator.CreateInstance(tombstoneType);
//                CopyToTombstoneEntity(entityType, tombstoneType, original, tombstone);
//                object existingTombstone = GetEntityBySurrogateKey(tombstoneType, surrogateKey.GetValue(entity, null), false);
//                if (existingTombstone != null)
//                {
//                    DB.Remove(existingTombstone);
//                }
//                DB.Add(tombstone);
//            }
//            DB.Remove(original);

//            List<LinqFunnelChangeResult> result = new List<LinqFunnelChangeResult>();
//            result.Add(new LinqFunnelChangeResult()
//            {
//                SurrogateKey = surrogateKeyValue,
//                EntityIdentifier = entityIdentifier,
//                Function = "DELETE",
//                DateChanged = DateTime.Now,
//                EntityChanged = entityType.Name,
//                FieldChanged = surrogateKey.Name,
//            }); ;
//            return result;
//        }

//        /// <summary>
//        /// Deletes an entity from the table correspoding to the entity type (E) and creates
//        /// a tombstone in the table correspoging to the tombstone entity type (T) if the 
//        /// createTombstone flag is set to true.
//        /// </summary>
//        /// <param name="entity">The entity to be deleted</param>
//        /// <param name="entityIdentifier">Unique value identifying the entity.</param>
//        /// <param name="createTombstone">Indicates whether a tombstone should be created.</param>
//        /// <param name="tombstoneType">The tombstone entity type i.e. the table where an tombstone will be created.</param>
//        /// <returns>Returns a list of change results.</returns>
//        public virtual List<LinqFunnelChangeResult> Delete(object entity, object entityIdentifier, bool createTombstone, Type tombstoneType)
//        {
//            List<LinqFunnelChangeResult> result = DeleteWithoutSavingToDb(entity, entityIdentifier, createTombstone, tombstoneType);
//            DB.SaveChanges();
//            return result;
//        }

//        /// <summary>
//        /// Deletes an entity from the table correspoding to the entity type (E) and creates
//        /// a tombstone in the table correspoging to the tombstone entity type (T) if the 
//        /// createTombstone flag is set to true.
//        /// </summary>
//        /// <param name="entity">The entity to be deleted</param>
//        /// <param name="entityIdentifier">Unique value identifying the entity.</param>
//        /// <param name="createTombstone">Indicates whether a tombstone should be created.</param>
//        /// <param name="tombstoneType">The tombstone entity type i.e. the table where an tombstone will be created.</param>
//        /// <returns>Returns a list of change results.</returns>
//        public async virtual Task<List<LinqFunnelChangeResult>> DeleteAsync(object entity, object entityIdentifier, bool createTombstone, Type tombstoneType)
//        {
//            List<LinqFunnelChangeResult> result = DeleteWithoutSavingToDb(entity, entityIdentifier, createTombstone, tombstoneType);
//            await DB.SaveChangesAsync();
//            return result;
//        }

//        /// <summary>
//        /// Deletes an entity from the table corresponding to the entity type.
//        /// </summary>
//        /// <typeparam name="E">The entity type i.e. which table it will be deleted from.</typeparam>
//        /// <param name="surrogateKeyValue">The entity to be deleted.</param>
//        /// <param name="entityIdentifier">Unique value identifying the entity.</param>
//        /// <returns>Returns a list of change results.</returns>
//        public virtual List<LinqFunnelChangeResult> DeleteBySurrogateKey<E>(object surrogateKeyValue, object entityIdentifier) where E : class
//        {
//            return DeleteBySurrogateKey<E, object>(surrogateKeyValue, entityIdentifier, false);
//        }

//        /// <summary>
//        /// Deletes an entity from the table corresponding to the entity type.
//        /// </summary>
//        /// <param name="surrogateKeyValue">The surrogate key value uniquely identifying the entity to be deleted.</param>
//        /// <param name="entityIdentifier">Unique value identifying the entity.</param>
//        /// <param name="entityType">The type of the entity to be deleted.</param>
//        /// <returns>Returns a list of change results.</returns>
//        public virtual List<LinqFunnelChangeResult> DeleteBySurrogateKey(object surrogateKeyValue, object entityIdentifier, Type entityType)
//        {
//            List<LinqFunnelChangeResult> result = DeleteBySurrogateKeyWithoutSavingToDb(surrogateKeyValue, entityIdentifier, false, entityType, null);
//            DB.SaveChanges();
//            return result;
//        }

//        /// <summary>
//        /// Deletes an entity from the table with the specified surrogate key correspoding to the entity type (E) and creates
//        /// a tombstone in the table correspoging to the tombstone entity type (T) if the 
//        /// createTombstone flag is set to true.
//        /// </summary>
//        /// <typeparam name="E">The entity type of the entity which will be deleted i.e. the table from where it will be deleted.</typeparam>
//        /// <typeparam name="T">The tombstone entity type i.e. the table where an tombstone will be created.</typeparam>
//        /// <param name="surrogateKeyValue">The surrogate key of the entity to be deleted</param>
//        /// <param name="entityIdentifier">Unique value identifying the entity.</param>
//        /// <param name="createTombstone">Indicates whether a tombstone should be created.</param>
//        /// <returns>Returns a list of change results.</returns>
//        public virtual List<LinqFunnelChangeResult> DeleteBySurrogateKey<E, T>(object surrogateKeyValue, object entityIdentifier, bool createTombstone)
//            where E : class
//            where T : class
//        {
//            List<LinqFunnelChangeResult> result = DeleteBySurrogateKeyWithoutSavingToDb(surrogateKeyValue, entityIdentifier, createTombstone, typeof(E), typeof(T));
//            DB.SaveChanges();
//            return result;
//        }

//        /// <summary>
//        /// Deletes an entity from the table with the specified surrogate key correspoding to the entity type (E) and creates
//        /// a tombstone in the table correspoging to the tombstone entity type (T) if the 
//        /// createTombstone flag is set to true.
//        /// </summary>
//        /// <param name="surrogateKeyValue">The surrogate key of the entity to be deleted</param>
//        /// <param name="entityIdentifier">Unique value identifying the entity.</param>
//        /// <param name="createTombstone">Indicates whether a tombstone should be created.</param>
//        /// <param name="entityType">The type of the entity to be deleted.</param>
//        /// <param name="tombstoneType">The type of the entity serving as the tombstone where the entity will be archited i.e. the tombstone table.</param>
//        /// <returns>Returns a list of change results.</returns>
//        protected virtual List<LinqFunnelChangeResult> DeleteBySurrogateKeyWithoutSavingToDb(
//            object surrogateKeyValue,
//            object entityIdentifier,
//            bool createTombstone,
//            Type entityType,
//            Type tombstoneType)
//        {
//            PropertyInfo surrogateKey = GetEntitySurrogateKey(entityType);
//            object original = GetEntityBySurrogateKey(entityType, surrogateKeyValue, false);
//            if (original == null)
//            {
//                throw new Exception(
//                    string.Format("Could not find entity with key {0} and value {1} to delete.",
//                    GetEntitySurrogateKey(entityType).Name,
//                    surrogateKeyValue));
//            }
//            if (createTombstone)
//            {
//                object tombstone = Activator.CreateInstance(tombstoneType);
//                CopyToTombstoneEntity(original, tombstone);
//                object existingTombstone = GetEntityBySurrogateKey(tombstoneType, surrogateKeyValue, false);
//                if (existingTombstone != null)
//                {
//                    DB.Remove(existingTombstone);
//                }
//                DB.Add(tombstone);
//            }
//            DB.Add(original);

//            List<LinqFunnelChangeResult> result = new List<LinqFunnelChangeResult>();
//            result.Add(new LinqFunnelChangeResult()
//            {
//                SurrogateKey = surrogateKeyValue,
//                EntityIdentifier = entityIdentifier,
//                Function = "DELETE",
//                DateChanged = DateTime.Now,
//                EntityChanged = entityType.Name,
//                FieldChanged = surrogateKey.Name,
//            });
//            return result;
//        }

//        /// <summary>
//        /// Deletes an entity from the table with the specified surrogate key correspoding to the entity type (E) and creates
//        /// a tombstone in the table correspoging to the tombstone entity type (T) if the 
//        /// createTombstone flag is set to true.
//        /// </summary>
//        /// <typeparam name="E">The entity type of the entity which will be deleted i.e. the table from where it will be deleted.</typeparam>
//        /// <typeparam name="T">The tombstone entity type i.e. the table where an tombstone will be created.</typeparam>
//        /// <param name="surrogateKeyValue">The surrogate key of the entity to be deleted</param>
//        /// <param name="entityIdentifier">Unique value identifying the entity.</param>
//        /// <param name="createTombstone">Indicates whether a tombstone should be created.</param>
//        /// <param name="entityType">The type of the entity to be deleted.</param>
//        /// <param name="tombstoneType">The type of the entity serving as the tombstone where the entity will be archited i.e. the tombstone table.</param>
//        /// <returns>Returns a list of change results.</returns>
//        public virtual List<LinqFunnelChangeResult> DeleteBySurrogateKey(
//            object surrogateKeyValue,
//            object entityIdentifier,
//            bool createTombstone,
//            Type entityType,
//            Type tombstoneType)
//        {
//            List<LinqFunnelChangeResult> result = DeleteBySurrogateKeyWithoutSavingToDb(surrogateKeyValue, entityIdentifier, createTombstone, entityType, tombstoneType);
//            DB.SaveChanges();
//            return result;
//        }

//        /// <summary>
//        /// Deletes an entity from the table with the specified surrogate key correspoding to the entity type (E) and creates
//        /// a tombstone in the table correspoging to the tombstone entity type (T) if the 
//        /// createTombstone flag is set to true.
//        /// </summary>
//        /// <typeparam name="E">The entity type of the entity which will be deleted i.e. the table from where it will be deleted.</typeparam>
//        /// <typeparam name="T">The tombstone entity type i.e. the table where an tombstone will be created.</typeparam>
//        /// <param name="surrogateKeyValue">The surrogate key of the entity to be deleted</param>
//        /// <param name="entityIdentifier">Unique value identifying the entity.</param>
//        /// <param name="createTombstone">Indicates whether a tombstone should be created.</param>
//        /// <param name="entityType">The type of the entity to be deleted.</param>
//        /// <param name="tombstoneType">The type of the entity serving as the tombstone where the entity will be archited i.e. the tombstone table.</param>
//        /// <returns>Returns a list of change results.</returns>
//        public async virtual Task<List<LinqFunnelChangeResult>> DeleteBySurrogateKeyAsync(
//            object surrogateKeyValue,
//            object entityIdentifier,
//            bool createTombstone,
//            Type entityType,
//            Type tombstoneType)
//        {
//            List<LinqFunnelChangeResult> result = DeleteBySurrogateKeyWithoutSavingToDb(surrogateKeyValue, entityIdentifier, createTombstone, entityType, tombstoneType);
//            await DB.SaveChangesAsync();
//            return result;
//        }

//        /// <summary>
//        /// Deletes all the entities in a given table older than the time specified.
//        /// </summary>
//        /// <typeparam name="E">The entity type of the entity which will be deleted i.e. the table from where it will be deleted.</typeparam>
//        /// <param name="dateFieldName">The field name on the entity which must a date time field .</param>
//        /// <param name="time">The time relative to the current time i.e. current time subracted by the this time sets the threshhold for entities deleted.</param>
//        protected virtual List<LinqFunnelChangeResult> DeleteOlderThanWithoutSavingToDb<E>(string dateFieldName, TimeSpan time) where E : class
//        {
//            DateTime threshold = DateTime.Now.Subtract(time);
//            DbSet<E> table = DB.Set<E>();
//            Type entityType = typeof(E);
//            PropertyInfo dateField = entityType.GetProperty(dateFieldName);
//            if ((dateField == null) || (dateField.PropertyType != typeof(DateTime)))
//            {
//                throw new Exception(
//                    string.Format("Entity {0} does not contain the DateTime field with the name {1}",
//                    entityType.Name,
//                    dateFieldName));
//            }
//            List<object> toDelete = new List<object>();
//            List<LinqFunnelChangeResult> result = new List<LinqFunnelChangeResult>();
//            foreach (E entity in table)
//            {
//                if ((DateTime)dateField.GetValue(entity, null) < threshold)
//                {
//                    table.Remove(entity);
//                    PropertyInfo surrogateKey = GetEntitySurrogateKey<E>();
//                    object surrogateKeyValue = surrogateKey.GetValue(entity, null);
//                    result.Add(new LinqFunnelChangeResult()
//                    {
//                        SurrogateKey = surrogateKeyValue,
//                        EntityIdentifier = null,
//                        Function = "DELETE",
//                        DateChanged = DateTime.Now,
//                        EntityChanged = entityType.Name,
//                        FieldChanged = surrogateKey.Name,
//                    });
//                }
//            }
//            return result;
//        }

//        /// <summary>
//        /// Deletes all the entities in a given table older than the time specified.
//        /// </summary>
//        /// <typeparam name="E">The entity type of the entity which will be deleted i.e. the table from where it will be deleted.</typeparam>
//        /// <param name="dateFieldName">The field name on the entity which must a date time field .</param>
//        /// <param name="time">The time relative to the current time i.e. current time subracted by the this time sets the threshhold for entities deleted.</param>
//        public virtual List<LinqFunnelChangeResult> DeleteOlderThan<E>(string dateFieldName, TimeSpan time) where E : class
//        {
//            List<LinqFunnelChangeResult> result = this.DeleteOlderThan<E>(dateFieldName, time);
//            DB.SaveChanges();
//            return result;
//        }

//        /// <summary>
//        /// Deletes all the entities in a given table older than the time specified.
//        /// </summary>
//        /// <typeparam name="E">The entity type of the entity which will be deleted i.e. the table from where it will be deleted.</typeparam>
//        /// <param name="dateFieldName">The field name on the entity which must a date time field .</param>
//        /// <param name="time">The time relative to the current time i.e. current time subracted by the this time sets the threshhold for entities deleted.</param>
//        public async virtual Task<List<LinqFunnelChangeResult>> DeleteOlderThanAsync<E>(string dateFieldName, TimeSpan time) where E : class
//        {
//            List<LinqFunnelChangeResult> result = this.DeleteOlderThanWithoutSavingToDb<E>(dateFieldName, time);
//            await DB.SaveChangesAsync();
//            return result;
//        }

//        /// <summary>
//        /// Deletes all the entities in a given table older than the time specified.
//        /// </summary>
//        /// <param name="entityType">The Type of the entity to be deleted.</param>
//        /// <param name="dateFieldName">The field name on the entity which must a date time field .</param>
//        /// <param name="time">The time relative to the current time i.e. current time subracted by the this time sets the threshhold for entities deleted.</param>
//        protected virtual void DeleteOlderThanWithoutSavingToDb(Type entityType, string dateFieldName, TimeSpan time)
//        {
//            DateTime threshold = DateTime.Now.Subtract(time);
//            IQueryable table = DB.Set(entityType);
//            PropertyInfo dateField = entityType.GetProperty(dateFieldName);
//            if ((dateField == null) || (dateField.PropertyType != typeof(DateTime)))
//            {
//                throw new Exception(
//                    string.Format("Entity {0} does not contain the DateTime field with the name {1}",
//                    entityType.Name,
//                    dateFieldName));
//            }
//            foreach (object e in table)
//            {
//                if ((DateTime)dateField.GetValue(e, null) < threshold)
//                {
//                    DB.Remove(e);
//                }
//            }
//        }

//        /// <summary>
//        /// Deletes all the entities in a given table older than the time specified.
//        /// </summary>
//        /// <param name="entityType">The Type of the entity to be deleted.</param>
//        /// <param name="dateFieldName">The field name on the entity which must a date time field .</param>
//        /// <param name="time">The time relative to the current time i.e. current time subracted by the this time sets the threshhold for entities deleted.</param>
//        public virtual void DeleteOlderThan(Type entityType, string dateFieldName, TimeSpan time)
//        {
//            DeleteOlderThanWithoutSavingToDb(entityType, dateFieldName, time);
//            DB.SaveChanges();
//        }

//        /// <summary>
//        /// Deletes all the entities in a given table older than the time specified.
//        /// </summary>
//        /// <param name="entityType">The Type of the entity to be deleted.</param>
//        /// <param name="dateFieldName">The field name on the entity which must a date time field .</param>
//        /// <param name="time">The time relative to the current time i.e. current time subracted by the this time sets the threshhold for entities deleted.</param>
//        public async virtual void DeleteOlderThanAsync(Type entityType, string dateFieldName, TimeSpan time)
//        {
//            DeleteOlderThanWithoutSavingToDb(entityType, dateFieldName, time);
//            await DB.SaveChangesAsync();
//        }

//        /// <summary>
//        /// Deletes all entities from the table corresponding to the entity type.
//        /// </summary>
//        /// <typeparam name="E">The entity type i.e. of the table whose records (entities) will be deleted.</typeparam>
//        protected virtual List<LinqFunnelChangeResult> DeleteAllWithoutSavingToDb<E>() where E : class
//        {
//            DbSet<E> table = DB.Set<E>();
//            List<E> entities = new List<E>();
//            foreach (E e in table)
//            {
//                entities.Add(e);
//            }
//            table.RemoveRange(entities);
//            List<LinqFunnelChangeResult> result = new List<LinqFunnelChangeResult>();
//            result.Add(new LinqFunnelChangeResult()
//            {
//                Function = "DELETE ALL",
//                DateChanged = DateTime.Now,
//                EntityChanged = typeof(E).Name,
//            });
//            return result;
//        }

//        /// <summary>
//        /// Deletes all entities from the table corresponding to the entity type.
//        /// </summary>
//        /// <typeparam name="E">The entity type i.e. of the table whose records (entities) will be deleted.</typeparam>
//        public virtual List<LinqFunnelChangeResult> DeleteAll<E>() where E : class
//        {
//            List<LinqFunnelChangeResult> result = DeleteAllWithoutSavingToDb<E>();
//            DB.SaveChanges();
//            return result;
//        }

//        /// <summary>
//        /// Deletes all entities from the table corresponding to the entity type.
//        /// </summary>
//        /// <typeparam name="E">The entity type i.e. of the table whose records (entities) will be deleted.</typeparam>
//        public async virtual Task<List<LinqFunnelChangeResult>> DeleteAllAsync<E>() where E : class
//        {
//            List<LinqFunnelChangeResult> result = DeleteAllWithoutSavingToDb<E>();
//            await DB.SaveChangesAsync();
//            return result;
//        }

//        /// <summary>
//        /// Deletes all entities from the table corresponding to the entity type.
//        /// </summary>
//        /// <typeparam name="E">The entity type i.e. of the table whose records (entities) will be deleted.</typeparam>
//        public virtual List<LinqFunnelChangeResult> DeleteAll(Type entityType)
//        {
//            IQueryable table = DB.Set(entityType);
//            List<object> entities = new List<object>();
//            foreach (object e in table)
//            {
//                entities.Add(e);
//            }
//            DB.RemoveRange(entities);
//            DB.SaveChanges();

//            List<LinqFunnelChangeResult> result = new List<LinqFunnelChangeResult>();
//            result.Add(new LinqFunnelChangeResult()
//            {
//                Function = "DELETE ALL",
//                DateChanged = DateTime.Now,
//                EntityChanged = entityType.Name,
//            });
//            return result;
//        }

//        /// <summary>
//        /// Copies all the values from the original entity to a tombstone entity. If the fields/columns on the two entities
//        /// do not match an exception will be thrown.
//        /// </summary>
//        /// <param name="original">The original entity retrieved from the database.</param>
//        /// <param name="tombstone">The tombstone entity containing the same fields/columns as the original entity.</param>
//        public virtual void CopyToTombstoneEntity<E, T>(E original, T tombstone)
//            where E : class
//            where T : class
//        {
//            CopyToTombstoneEntity(typeof(E), typeof(T), original, tombstone);
//        }

//        /// <summary>
//        /// Copies all the values from the original entity to a tombstone entity. If the fields/columns on the two entities
//        /// do not match an exception will be thrown.
//        /// </summary>
//        /// <param name="entityType">The Type of the original entity to be copied to the tombstone entity.</param>
//        /// <param name="tombstoneType">The Type of the tombstone entity containing the same fields/columns as the original entity.</param>
//        /// <param name="original">The original entity retrieved from the database.</param>
//        /// <param name="tombstone">The tombstone entity containing the same fields/columns as the original entity.</param>
//        public virtual void CopyToTombstoneEntity(Type entityType, Type tombstoneType, object original, object tombstone)
//        {
//            PropertyInfo[] properties = entityType.GetProperties();
//            foreach (PropertyInfo p in properties)
//            {
//                object[] attributes = p.GetCustomAttributes(typeof(ColumnAttribute), false);
//                ColumnAttribute columnAttribute = attributes.Length < 1 ? null : (ColumnAttribute)attributes[0];
//                if (columnAttribute == null)
//                {
//                    continue;
//                }
//                PropertyInfo tombstoneProperty = tombstoneType.GetProperty(p.Name);
//                if (tombstoneProperty == null)
//                {
//                    throw new NullReferenceException(
//                        string.Format(
//                        "Could not find property on tombstone entity with the name {0}.",
//                        p.Name));
//                }
//                object originalValue = p.GetValue(original, null);
//                tombstoneProperty.SetValue(tombstone, originalValue, null);
//            }
//        }

//        /// <summary>
//        /// Queries for and returns the first entity filtered by the specified expression.
//        /// </summary>
//        /// <typeparam name="E">The entity type i.e. which table the entity will be queried from.</typeparam>
//        /// <param name="expression">The expression to use to filter by.</param>
//        /// <returns>Returns the first entity filtered by the specified expression.</returns>
//        public virtual E GetFirstEntity<E>(Expression<Func<E, bool>> expression) where E : class
//        {
//            return GetFirstEntity<E>(expression, false);
//        }

//        /// <summary>
//        /// Queries for and returns the first entity filtered by the specified expression.
//        /// </summary>
//        /// <typeparam name="E">The entity type i.e. which table the entity will be queried from.</typeparam>
//        /// <param name="expression">The expression to use to filter by.</param>
//        /// <param name="throwExceptionOnNotFound">Whether or not to to throw an exception if the result is null.</param>
//        /// <returns>Returns the first entity filtered by the specified expression.</returns>
//        public virtual E GetFirstEntity<E>(Expression<Func<E, bool>> expression, bool throwExceptionOnNotFound) where E : class
//        {
//            E result = DB.Set<E>().Where(expression.Compile()).FirstOrDefault();
//            if (result == null && throwExceptionOnNotFound)
//            {
//                throw new Exception(string.Format("Could not find {0} for expression {1}'.",
//                    typeof(E).Name,
//                    expression.ToString()));
//            }
//            return result;
//        }

//        /// <summary>
//        /// Queries for and returns a list of entities filtered by the specified expression.
//        /// </summary>
//        /// <typeparam name="E">The entity type i.e. which table the entity will be queried from.</typeparam>
//        /// <param name="expression">The expression to use to filter by.</param>
//        /// <returns>Returns a list of entities filtered by the specified expression.</returns>
//        public virtual List<E> GetEntities<E>(Expression<Func<E, bool>> expression) where E : class
//        {
//            return DB.Set<E>().Where(expression.Compile()).ToList();
//        }

//        /// <summary>
//        /// Queries for and returns an entity from the table corresponding to the entity type. The query is performed
//        /// on the surrogate key of the entity.
//        /// </summary>
//        /// <param name="entityType">The entity type i.e. which table the entity will be queried from.</param>
//        /// <param name="keyValue">The value of the surrogate to search by.</param>
//        /// <returns>Returns an entity with the specified type and surrogate key. Returns null if one is not found.</returns>
//        public virtual object GetEntityBySurrogateKey(Type entityType, object keyValue)
//        {
//            return GetEntityBySurrogateKey(entityType, keyValue, false);
//        }

//        /// <summary>
//        /// Queries for and returns an entity from the table corresponding to the entity type. The query is performed
//        /// on the surrogate key of the entity.
//        /// </summary>
//        /// <param name="entityType">The entity type i.e. which table the entity will be queried from.</param>
//        /// <param name="keyValue">The value of the surrogate to search by.</param>
//        /// <param name="throwExceptionOnNotFound">Whether or not to throw an exception if the result is null.</param>
//        /// <returns>Returns an entity with the specified type and surrogate key. Returns null if one is not found.</returns>
//        public virtual object GetEntityBySurrogateKey(Type entityType, object keyValue, bool throwExceptionOnNotFound)
//        {
//            MethodInfo methodDefinition = GetType().GetMethod("GetEntityBySurrogateKey", new Type[] { typeof(object), typeof(bool), typeof(bool) }); //https://stackoverflow.com/questions/266115/pass-an-instantiated-system-type-as-a-type-parameter-for-a-generic-class
//            MethodInfo method = methodDefinition.MakeGenericMethod(entityType);
//            return method.Invoke(this, new object[] { keyValue, throwExceptionOnNotFound });
//        }

//        /// <summary>
//        /// Queries for and returns an entity from the table corresponding to the entity type. The query is performed
//        /// on the surrogate key of the entity.
//        /// </summary>
//        /// <typeparam name="E">The entity type i.e. which table the entity will be queried from.</typeparam>
//        /// <param name="keyValue">The value of the surrogate to search by.</param>
//        /// <returns>Returns an entity with the specified type and surrogate key. Returns null if one is not found.</returns>
//        public virtual E GetEntityBySurrogateKey<E>(object keyValue) where E : class
//        {
//            return GetEntityBySurrogateKey<E>(keyValue, false);
//        }

//        /// <summary>
//        /// Queries for and returns an entity from the table corresponding to the entity type. The query is performed
//        /// on the surrogate key of the entity.
//        /// </summary>
//        /// <typeparam name="E">The entity type i.e. which table the entity will be queried from.</typeparam>
//        /// <param name="keyValue">The value of the surrogate to search by.</param>
//        /// <param name="throwExceptionOnNotFound">Whether or not to throw an exception if the result is null.</param>
//        /// <returns>Returns an entity with the specified type and surrogate key. Returns null if one is not found.</returns>
//        public virtual E GetEntityBySurrogateKey<E>(object keyValue, bool throwExceptionOnNotFound) where E : class
//        {
//            Type entityType = typeof(E);
//            PropertyInfo surrogateKey = GetEntitySurrogateKey(entityType);
//            object keyValueConverted = EntityReader.ConvertValueTypeTo(keyValue, surrogateKey.PropertyType);
//            ParameterExpression e = Expression.Parameter(entityType, "e");
//            MemberExpression memberExpression = Expression.MakeMemberAccess(e, surrogateKey); //Name of surrogate key : left hand side of the expression.
//            ConstantExpression constantExpression = Expression.Constant(keyValueConverted, surrogateKey.PropertyType); //Value of the surrogate key : right hand side of the expression.
//            BinaryExpression binaryExpression = Expression.Equal(memberExpression, constantExpression);
//            Expression<Func<E, bool>> lambdaExpression = Expression.Lambda<Func<E, bool>>(binaryExpression, e);
//            return DB.Set<E>().SingleOrDefault(lambdaExpression);
//        }

//        /// <summary>
//        /// Queries for entities in a table corresponding to entity type. The query is performed on the column/field
//        /// specified with the specified field value.
//        /// </summary>
//        /// <param name="entityType">The entity type i.e. the table from which the entities will be returned.</param>
//        /// <param name="fieldName">The name of the field/column on which the query will be performed.</param>
//        /// <param name="fieldValue">The value of the field which will be used for the query.</param>
//        /// <returns>Returns a list of entities of the specified type with the specified field/column and field value.</returns>
//        public virtual List<object> GetEntitiesByField(Type entityType, string fieldName, object fieldValue)
//        {
//            MethodInfo methodDefinition = GetType().GetMethod("GetEntitiesByField", new Type[] { typeof(string), typeof(object), typeof(bool) }); //https://stackoverflow.com/questions/266115/pass-an-instantiated-system-type-as-a-type-parameter-for-a-generic-class
//            MethodInfo method = methodDefinition.MakeGenericMethod(entityType);
//            object queryResult = method.Invoke(this, new object[] { fieldName, fieldValue });
//            System.Collections.IList genericList = (System.Collections.IList)queryResult;
//            List<object> result = new List<object>();
//            foreach (var e in genericList)
//            {
//                result.Add(e);
//            }
//            return result;
//        }

//        /// <summary>
//        /// Queries for entities in a table corresponding to entity type. The query is performed on the column/field
//        /// specified with the specified field value.
//        /// </summary>
//        /// <typeparam name="E">The entity type i.e. the table from which the entities will be returned.</typeparam>
//        /// <param name="fieldName">The name of the field/column on which the query will be performed.</param>
//        /// <param name="fieldValue">The value of the field which will be used for the query.</param>
//        /// <returns>Returns a list of entities of the specified type with the specified field/column and field value.</returns>
//        public virtual List<E> GetEntitiesByField<E>(string fieldName, object fieldValue) where E : class
//        {
//            Type entityType = typeof(E);
//            PropertyInfo field = entityType.GetProperty(fieldName);
//            object keyValueConverted = EntityReader.ConvertValueTypeTo(fieldValue, field.PropertyType);
//            ParameterExpression e = Expression.Parameter(entityType, "e");
//            MemberExpression memberExpression = Expression.MakeMemberAccess(e, field); //Name of surrogate key : left hand side of the expression.
//            ConstantExpression constantExpression = Expression.Constant(keyValueConverted, field.PropertyType); //Value of the surrogate key : right hand side of the expression.
//            BinaryExpression binaryExpression = Expression.Equal(memberExpression, constantExpression);
//            Expression<Func<E, bool>> lambdaExpression = Expression.Lambda<Func<E, bool>>(binaryExpression, e);
//            return DB.Set<E>().Where(lambdaExpression).ToList();
//        }

//        /// <summary>
//        /// Queries for all the entities in a table corresponging to the specfied entity type.
//        /// </summary>
//        /// <typeparam name="E">The entity type i.e. the table from which the entities will be returned.</typeparam>
//        /// <returns>Returns all the entities of the specified type.</returns>
//        public virtual List<E> GetAllEntities<E>() where E : class
//        {
//            return DB.Set<E>().ToList<E>();
//        }

//        /// <summary>
//        /// Queries for all the entities in a table corresponging to the specfied entity type.
//        /// </summary>
//        /// <typeparam name="E">The entity type i.e. the table from which the entities will be returned.</typeparam>
//        /// <returns>Returns all the entities of the specified type.</returns>
//        public virtual List<object> GetAllEntities(Type entityType)
//        {
//            List<object> result = new List<object>();
//            IQueryable table = DB.Set(entityType);
//            foreach (object e in table)
//            {
//                result.Add(e);
//            }
//            return result;
//        }

//        /// <summary>
//        /// Returns the total count of an entity type.
//        /// </summary>
//        /// <typeparam name="E">The entity type i.e. the table from which the entities will be counted.</typeparam>
//        /// <returns>Returns the total count of an entity type.</returns>
//        public virtual int GetTotalCount<E>() where E : class
//        {
//            return DB.Set<E>().Count();
//        }

//        /// <summary>
//        /// Runs a raw SQL query to the database to count the number of records in a specified database table.
//        /// </summary>
//        /// <param name="entityType">The entity type i.e. the table from which the entities will be counted.</param>
//        /// <returns>Returns the total count of an entity type.</returns>
//        public virtual int GetTotalCount(Type entityType)
//        {
//            int result = 0;
//            using (var command = DB.Database.GetDbConnection().CreateCommand())
//            {
//                command.CommandText = $"SELECT COUNT(*) FROM {entityType.Name}";
//                command.CommandType = CommandType.Text;
//                DB.Database.OpenConnection();
//                using (DbDataReader dbDataReader = command.ExecuteReader())
//                {
//                    while (dbDataReader.Read())
//                    {
//                        result = Convert.ToInt32(dbDataReader[0]);
//                    }
//                }
//            }
//            return result;
//        }

//        /// <summary>
//        /// Returns the total count of an entity type.
//        /// </summary>
//        /// <typeparam name="E">The entity type i.e. the table from which the entities will be counted.</typeparam>
//        /// <returns>Returns the total count of an entity type.</returns>
//        public virtual long GetTotalCountLong<E>() where E : class
//        {
//            return DB.Set<E>().LongCount();
//        }

//        /// <summary>
//        /// Runs a raw SQL query to the database to count the number of records in a specified database table.
//        /// </summary>
//        /// <param name="entityType">The entity type i.e. the table from which the entities will be counted.</param>
//        /// <returns>Returns the total count of an entity type.</returns>
//        public virtual long GetTotalCountLong(Type entityType)
//        {
//            long result = 0;
//            using (var command = DB.Database.GetDbConnection().CreateCommand())
//            {
//                command.CommandText = $"SELECT COUNT(*) FROM {entityType.Name}";
//                command.CommandType = CommandType.Text;
//                DB.Database.OpenConnection();
//                using (DbDataReader dbDataReader = command.ExecuteReader())
//                {
//                    while (dbDataReader.Read())
//                    {
//                        result = Convert.ToInt64(dbDataReader[0]);
//                    }
//                }
//            }
//            return result;
//        }

//        /// <summary>
//        /// Logs the Exception to the NKitLogEntry table if the table exists in the database.
//        /// A sql query is run against the database first to check that the table exists in the database before trying to insert a log entry to it.
//        /// To ensure that the table exists the NKitLogEntry model needs to registered by underlying DbContext as a DbSet in the application using the NKit.
//        /// </summary>
//        public virtual NKitLogEntry LogExceptionToNKitLogEntry(Exception ex, Nullable<EventId> eventId, bool includeExceptionDetailsInErrorMessage)
//        {
//            if (!_loggingSettings.LogToNKitLogEntryDatabaseTable || !SqlTableExists(nameof(NKitLogEntry)))
//            {
//                return null;
//            }
//            string message = ExceptionHandler.GetCompleteExceptionMessage(ex, includeExceptionDetailsInErrorMessage);
//            NKitLogEntry logEntry = new NKitLogEntry()
//            {
//                NKitLogEntryId = Guid.NewGuid(),
//                Message = message,
//                Source = ex.Source,
//                ClassName = ex.TargetSite != null ? ex.TargetSite.DeclaringType.FullName : null,
//                FunctionName = ex.TargetSite != null ? ex.TargetSite.Name : null,
//                StackTrace = ex.StackTrace != null ? ex.StackTrace : null,
//                EventId = eventId.HasValue ? eventId.Value.Id : 0,
//                EventName = eventId.HasValue ? eventId.Value.Name : null,
//                DateCreated = DateTime.Now
//            };
//            List<LinqFunnelChangeResult> changeResult = Insert<NKitLogEntry>(logEntry, logEntry.NKitLogEntryId);
//            return logEntry;
//        }

//        public virtual NKitLogEntry LogWebActionActivityToNKitLogEntry(string className, string actionName, string message, Nullable<EventId> eventId)
//        {
//            if (!_loggingSettings.LogToNKitLogEntryDatabaseTable || !SqlTableExists(nameof(NKitLogEntry)))
//            {
//                return null;
//            }
//            NKitLogEntry logEntry = new NKitLogEntry()
//            {
//                NKitLogEntryId = Guid.NewGuid(),
//                Message = message,
//                Source = _generalSettings.ApplicationName,
//                ClassName = className,
//                FunctionName = actionName ?? "Request",
//                StackTrace = null,
//                EventId = eventId.HasValue ? eventId.Value.Id : 0,
//                EventName = eventId.HasValue ? eventId.Value.Name : null,
//                DateCreated = DateTime.Now
//            };
//            List<LinqFunnelChangeResult> changeResult = Insert<NKitLogEntry>(logEntry, logEntry.NKitLogEntryId);
//            return logEntry;
//        }

//        public virtual bool SqlTableExists(string tableName)
//        {
//            switch (_dbRepositorySettings.DatabaseProviderName)
//            {
//                case NKitDbProviderName.SqlServer:
//                    return SqlServerTableExists(tableName);
//                case NKitDbProviderName.Sqlite:
//                    return SqliteTableExists(tableName);
//                default:
//                    throw new ArgumentException($"Unsupported {nameof(NKitDbProviderName)} of {_dbRepositorySettings.DatabaseProviderName} set in {nameof(_dbRepositorySettings.DatabaseProviderName)}.");
//            }
//        }

//        public virtual bool SqlServerTableExists(string tableName)
//        {
//            string sqlQuery = $"SELECT COUNT(*) as Count FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}'";
//            DbConnection connection = DB.Database.GetDbConnection();
//            if (connection == null)
//            {
//                return false;
//            }
//            int count = connection.Query<int>(sqlQuery).FirstOrDefault(); // Query method extension provided by Dapper library.
//            return (count > 0);
//        }

//        public virtual bool SqliteTableExists(string tableName)
//        {
//            string sqlQuery = $"SELECT name FROM sqlite_master WHERE type = 'table' AND name = '{tableName}'";
//            DbConnection connection = DB.Database.GetDbConnection();
//            if (connection == null)
//            {
//                return false;
//            }
//            string queryResult = connection.Query<string>(sqlQuery).FirstOrDefault(); // Query method extension provided by Dapper library.
//            return !string.IsNullOrEmpty(queryResult) && queryResult.Equals(tableName);
//        }

//        /// <summary>
//        /// Returns the total count of an entity type.
//        /// </summary>
//        /// <typeparam name="E">The entity type i.e. the table from which the entities will be counted.</typeparam>
//        /// <returns>Returns the total count of an entity type.</returns>
//        //public virtual int GetTotalCount(Type entityType)
//        //{
//        //    return GetAllEntities(entityType, false).Count;
//        //}

//        /// <summary>
//        /// Returns the total count of an entity type.
//        /// </summary>
//        /// <typeparam name="E">The entity type i.e. the table from which the entities will be counted.</typeparam>
//        /// <returns>Returns the total count of an entity type.</returns>
//        //public virtual long GetTotalCountLong(Type entityType)
//        //{
//        //    return GetAllEntities(entityType, false).LongCount();
//        //}

//        /// <summary>
//        /// Disposes the underlying Entity Framework DbContext.
//        /// </summary>
//        public void Dispose()
//        {
//            if (_db != null)
//            {
//                _db.Dispose();
//            }
//        }

//        #endregion //Methods
//    }
//}
