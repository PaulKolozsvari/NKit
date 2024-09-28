namespace NKit.Web.Service.RestApi
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using NKit.Data.DB.LINQ;
    using NKit.Settings.Default;
    using NKit.Utilities;
    using NKit.Utilities.Email;
    using NKit.Web.Client;
    using NKit.Web.Service.RestApi.Events;
    using ZXing;

    #endregion //Using Directives

    /// <summary>
    /// CRUD (Createm, Read, Update, Delete) that inherits from the NKitWebApiController, but provides additional CRUD (Create, Read, Update, Delete) actions.
    /// It can be useful when you do want to expose the underlying database's data through CRUD actions.
    /// N.B. Must register HttpContextAccessor as a service in Startup.ConfigureServices of your app: services.AddHttpContextAccessor();
    /// </summary>
    /// <typeparam name="D">The NKitDbRepository that manages an underlying DbContext.</typeparam>
    [ApiController]
    public class NKitWebApiControllerCrud<D> : NKitWebApiController<D> where D : NKitDbContext
    {
        #region Constructors

        public NKitWebApiControllerCrud(
            D dbContext,
            IHttpContextAccessor
            httpContextAccessor,
            IOptions<NKitGeneralSettings> generalOptions,
            IOptions<NKitWebApiControllerSettings> webApiControllerOptions,
            IOptions<NKitDbContextSettings> databaseOptions,
            IOptions<NKitEmailClientServiceSettings> emailOptions,
            IOptions<NKitLoggingSettings> loggingOptions,
            IOptions<NKitWebApiClientSettings> webApiClientOptions,
            NKitEmailClientService emailClientService,
            ILogger logger,
            IWebHostEnvironment environment) :
            base(dbContext, httpContextAccessor, generalOptions, webApiControllerOptions, databaseOptions, emailOptions, loggingOptions, webApiClientOptions, emailClientService, logger, environment)
        {
        }

        #endregion //Constructors

        #region Events

        /// <summary>
        /// Event fired on request for get entities before the query is run against the database.
        /// </summary>
        public event OnBeforeGetEntitiesHandler OnBeforeGetEntities;

        /// <summary>
        /// Event fired on request for get entities after the query is run against the database.
        /// </summary>
        public event OnAfterGetEntitiesHandler OnAfterGetEntities;

        /// <summary>
        /// Event fired on request for getting an entity by ID, before the query is run against the database.
        /// </summary>
        public event OnBeforeGetEntityByIdHandler OnBeforeGetEntityById;

        /// <summary>
        /// Event fired on request for getting an entity by ID, after the query is run against the database.
        /// </summary>
        protected event OnAfterGetEntityByIdHandler OnAfterGetEntityById;

        /// <summary>
        /// Event fired on request for putting (saving) an entity, before the query is run against the database.
        /// </summary>
        public event OnBeforePutEntityHandler OnBeforePut;

        /// <summary>
        /// Event fired on request for putting (saving) an entity, after the query is run against the database.
        /// </summary>
        public event OnAfterPutEntityHandler OnAfterPut;

        /// <summary>
        /// Event fired on request for posting (inserting) an entity, before the query is run against the database.
        /// </summary>
        public event OnBeforePostEntityHandler OnBeforePost;

        /// <summary>
        /// Event fired on request for posting (inserting) an entity, after the query is run against the database.
        /// </summary>
        public event OnAfterPostEntityHandler OnAfterPost;

        /// <summary>
        /// Event fired on request for deleting an entity, before the query is run against the database.
        /// </summary>
        public event OnBeforeDeleteEntityHandler OnBeforeDelete;

        /// <summary>
        /// Event fired on request for deleting an entity, after the query is run against the database.
        /// </summary>
        public event OnAfterDeleteEntityHandler OnAfterDelete;

        /// <summary>
        /// Event fired on request for deleting all entities of a specific type, before the query is run against the database.
        /// </summary>
        public event OnBeforeDeleteAllEntitiesHandler OnBeforeDeleteAll;

        /// <summary>
        /// Event fired on request for deleting all entities of a specific type, after the query is run against the database.
        /// </summary>
        public event OnAfterDeleteAllEntitiesHandler OnAfterDeleteAll;

        #endregion //Events

        #region Actions

        /// <summary>
        /// Get a single entity (table record) of the the specified entity name (table name) by searching for its surrogate key (single table key) supplied in the entityId parameter.
        /// This action is dynamic in that any database table can be specified as the entity name as long as it's been added to the DbContext as a DbSet.
        /// Returns a single record in JSON or XML format based on the configured SerializerType property set in the NKitWebApiControllerSettings section of the appsettings.json file.
        /// The Content-Type of the response is based on the configured ResponseContentType property in the NKitWebApiControllerSettings section of the appsettings.json file.
        /// Called as such: "/{entityName}/{entityId}"
        /// </summary>
        /// <param name="entityName">The name of the database table to query from.</param>
        /// <param name="entityId">The surrogate key (single table key) of the database record.</param>
        /// <returns>Returns a single record in JSON or XML format based on the configured SerializerType property set in the NKitWebApiControllerSettings section of the appsettings.json file.</returns>
        [HttpGet, Route("{entityName}/{entityId}")]
        [Produces(MimeContentType.TEXT_PLAIN, MimeContentType.APPLICATION_JSON, MimeContentType.APPLICATION_XML)]
        public virtual IActionResult GetEntityById(string entityName, string entityId)
        {
            try
            {
                string requestName = $"{nameof(GetEntityById)} : Entity Name = {entityName} : Entity ID = {entityId}";
                string responseMessage = null;
                LogWebRequest(requestName, null);
                ValidateRequestMethod(HttpVerb.GET);
                string userName = GetCurrentUserName();
                Type entityType = GetEntityType(entityName);
                if (OnBeforeGetEntityById != null)
                {
                    var e = new NKitRestApiGetEntityByIdEventArgs(entityName, userName, DbContext, entityType, entityId, null);
                    OnBeforeGetEntityById(this, e);
                    if (e.Cancel)
                    {
                        responseMessage = string.Format("{0} get by id cancelled.", entityName);
                        LogWebResponse(requestName, responseMessage);
                        return Ok(responseMessage);
                    }
                }
                object outputEntity = DbContext.GetEntityBySurrogateKey(entityType, entityId);
                if (OnAfterGetEntityById != null)
                {
                    OnAfterGetEntityById(this, new NKitRestApiGetEntityByIdEventArgs(entityName, userName, DbContext, entityType, entityId, outputEntity));
                }
                string serializedText = GetSerializer().SerializeToText(outputEntity, GetNKitSerializerModelTypes());
                LogWebResponse(requestName, serializedText);
                Response.ContentType = WebApiControllerSettings.ResponseContentType;
                return Ok(serializedText);
            }
            finally
            {
                DisposeEntityContext();
            }
        }

        //Using Query Parameters: https://stackoverflow.com/questions/59621208/how-do-i-use-query-parameters-in-attributes
        /// <summary>
        /// Gets entities (table records) of the the specified entity name (table name). 
        /// This action is dynamic in that any database table can be specified as the entity name as long as it's been added to the DbContext as a DbSet.
        /// Optional search by field and search value of can be applied to apply a filter based on a specific field and its value i.e. searching by a specific column in the database table.
        /// Returns as a list of the specified entity type in either JSON or XML format based on the configured SerializerType property set in the NKitWebApiControllerSettings section of the appsettings.json file.
        /// The Content-Type of the response is based on the configured ResponseContentType property in the NKitWebApiControllerSettings section of the appsettings.json file.
        /// Called as such: "/{entityName}?{fieldName}&amp;searchValueOf={fieldValue}"
        /// </summary>
        /// <param name="entityName">The name of the database table to query from.</param>
        /// <param name="searchBy">Optional name of a column to filter by.</param>
        /// <param name="searchValueOf">Optional value of the column that is being filtered by.</param>
        /// <returns>Returns as a list of the specified entity type in either JSON or XML format based on the configured SerializerType property set in the NKitWebApiControllerSettings section of the appsettings.json file.</returns>
        [HttpGet, Route("{entityName}")]
        [Produces(MimeContentType.TEXT_PLAIN, MimeContentType.APPLICATION_JSON, MimeContentType.APPLICATION_XML)]
        public virtual IActionResult GetEntities([FromRoute] string entityName, [FromQuery] string searchBy, [FromQuery] string searchValueOf)
        {
            try
            {
                string requestName = $"{nameof(GetEntities)} : Entity Name = {entityName} : Search by = {searchBy} : Search by value = {searchValueOf}";
                string responseMessage = null;
                LogWebRequest(requestName, null);
                ValidateRequestMethod(HttpVerb.GET);
                string userName = GetCurrentUserName();
                Type entityType = GetEntityType(entityName);
                if (OnBeforeGetEntities != null)
                {
                    var e = new NKitRestApiGetEntitiesEventArgs(entityName, userName, DbContext, entityType, searchBy, searchValueOf, null);
                    OnBeforeGetEntities(this, e);
                    if (e.Cancel)
                    {
                        responseMessage = string.Format("{0} get all cancelled.", entityName);
                        LogWebResponse(requestName, responseMessage);
                        return Ok(responseMessage);
                    }
                }
                List<object> outputEntities = string.IsNullOrEmpty(searchBy) ?
                    DbContext.GetAllEntities(entityType) :
                    DbContext.GetEntitiesByField(entityType, searchBy, searchValueOf);
                if (OnAfterGetEntities != null)
                {
                    OnAfterGetEntities(this, new NKitRestApiGetEntitiesEventArgs(entityName, userName, DbContext, entityType, searchBy, searchValueOf, outputEntities));
                }
                string serializedText = GetSerializer().SerializeToText(outputEntities, GetNKitSerializerModelTypes());
                LogWebResponse(requestName, serializedText);
                Response.ContentType = WebApiControllerSettings.ResponseContentType;
                return Ok(serializedText);
            }
            finally
            {
                DisposeEntityContext();
            }
        }

        /// <summary>
        /// Gets the total count of entities (table records).
        /// Returns an integer (Int32) in either JSON or XML format based on the configured SerializerType property set in the NKitWebApiControllerSettings section of the appsettings.json file.
        /// The Content-Type of the response is based on the configured ResponseContentType property in the NKitWebApiControllerSettings section of the appsettings.json file.
        /// Called as such: "/{entityName}/Count"
        /// </summary>
        /// <param name="entityName">The name of the database table to query from.</param>
        /// <returns>Returns an integer (Int32) in either JSON or XML format based on the configured SerializerType property set in the NKitWebApiControllerSettings section of the appsettings.json file.</returns>
        [HttpGet, Route("{entityName}/Count")]
        [Produces(MimeContentType.TEXT_PLAIN, MimeContentType.APPLICATION_JSON, MimeContentType.APPLICATION_XML)]
        public virtual IActionResult GetEntityCount([FromRoute] string entityName)
        {
            try
            {
                string requestName = $"{nameof(GetEntityCount)} : Entity Name = {entityName}";
                string responseMessage = null;
                LogWebRequest(requestName, null);
                ValidateRequestMethod(HttpVerb.GET);
                string userName = GetCurrentUserName();
                Type entityType = GetEntityType(entityName);
                if (OnBeforeGetEntities != null)
                {
                    var e = new NKitRestApiGetEntitiesEventArgs(entityName, userName, DbContext, entityType, null, null, null);
                    OnBeforeGetEntities(this, e);
                    if (e.Cancel)
                    {
                        responseMessage = string.Format("{0} get count cancelled.", entityName);
                        LogWebResponse(requestName, responseMessage);
                        return Ok(responseMessage);
                    }
                }
                int result = DbContext.GetTotalCount(entityType);
                if (OnAfterGetEntities != null)
                {
                    OnAfterGetEntities(this, new NKitRestApiGetEntitiesEventArgs(entityName, userName, DbContext, entityType, null, null, new List<object>() { result }));
                }
                string serializedText = GetSerializer().SerializeToText(result, GetNKitSerializerModelTypes());
                LogWebResponse(requestName, serializedText);
                Response.ContentType = WebApiControllerSettings.ResponseContentType;
                return Ok(serializedText);
            }
            finally
            {
                DisposeEntityContext();
            }
        }

        /// <summary>
        /// Gets the total count of entities (table records).
        /// Returns an long (Int64) in either JSON or XML format based on the configured SerializerType property set in the NKitWebApiControllerSettings section of the appsettings.json file.
        /// The Content-Type of the response is based on the configured ResponseContentType property in the NKitWebApiControllerSettings section of the appsettings.json file.
        /// Called as such: "/{entityName}/Count"
        /// </summary>
        /// <param name="entityName">The name of the database table to query from.</param>
        /// <returns>Returns an integer (Int64) in either JSON or XML format based on the configured SerializerType property set in the NKitWebApiControllerSettings section of the appsettings.json file.</returns>
        [HttpGet, Route("{entityName}/CountLong")]
        [Produces(MimeContentType.TEXT_PLAIN, MimeContentType.APPLICATION_JSON, MimeContentType.APPLICATION_XML)]
        public virtual IActionResult GetEntityCountLong([FromRoute] string entityName)
        {
            try
            {
                string requestName = $"{nameof(GetEntityCount)} : Entity Name = {entityName}";
                string responseMessage = null;
                LogWebRequest(requestName, null);
                ValidateRequestMethod(HttpVerb.GET);
                string userName = GetCurrentUserName();
                Type entityType = GetEntityType(entityName);
                if (OnBeforeGetEntities != null)
                {
                    var e = new NKitRestApiGetEntitiesEventArgs(entityName, userName, DbContext, entityType, null, null, null);
                    OnBeforeGetEntities(this, e);
                    if (e.Cancel)
                    {
                        responseMessage = string.Format("{0} get long count cancelled.", entityName);
                        LogWebResponse(requestName, responseMessage);
                        return Ok(responseMessage);
                    }
                }
                long result = DbContext.GetTotalCountLong(entityType);
                if (OnAfterGetEntities != null)
                {
                    OnAfterGetEntities(this, new NKitRestApiGetEntitiesEventArgs(entityName, userName, DbContext, entityType, null, null, new List<object>() { result }));
                }
                string serializedText = GetSerializer().SerializeToText(result, GetNKitSerializerModelTypes());
                LogWebResponse(requestName, serializedText);
                Response.ContentType = WebApiControllerSettings.ResponseContentType;
                return Ok(serializedText);
            }
            finally
            {
                DisposeEntityContext();
            }
        }

        /// <summary>
        /// Saves (updates or inserts) a single entity to a database table matching the entity name. 
        /// This action is dynamic in that any database table can be specified as the entity name as long as it's been added to the DbContext as a DbSet.
        /// The request body can be in either JSON or XML format based on the configured SerializerType property set in the NKitWebApiControllerSettings section of the appsettings.json file.
        /// The request body can be text/plain, application/json or application/xml.
        /// Returns a text message simple message indicating that the entity has been saved. 
        /// The Content-Type of the is always text/plain.
        /// Called as such: "/{entityName}" with the JSON or XML contents as part of the request body.
        /// </summary>
        /// <param name="entityName">The name of the database table to save to.</param>
        /// <param name="serializedText">The JSON or XML representation of the entity (record) being saved.</param>
        /// <returns>Returns a text message simple message indicating that the entity has been saved. </returns>
        [HttpPut, Route("{entityName}")]
        [Consumes(MimeContentType.TEXT_PLAIN, MimeContentType.APPLICATION_JSON, MimeContentType.APPLICATION_XML)]
        [Produces(MimeContentType.TEXT_PLAIN)]
        public virtual IActionResult PutEntity(string entityName, [FromBody] string serializedText)
        {
            try
            {
                string requestName = $"{nameof(PutEntity)} : Entity Name : {entityName}";
                string responseMessage = null;
                ValidateRequestMethod(HttpVerb.PUT);
                string userName = GetCurrentUserName();
                Type entityType = GetEntityType(entityName);
                object inputEntity = GetSerializer().DeserializeFromText(entityType, GetNKitSerializerModelTypes(), serializedText);
                LogWebRequest(requestName, serializedText);
                if (OnBeforePut != null)
                {
                    var e = new NKitRestApiPutEntityEventArgs(entityName, userName, DbContext, entityType, inputEntity);
                    OnBeforePut(this, e);
                    if (e.Cancel)
                    {
                        responseMessage = string.Format("{0} saving cancelled.", entityName);
                        LogWebResponse(requestName, responseMessage);
                        return Ok(responseMessage);
                    }
                }
                DbContext.Save(entityType, inputEntity, null);
                if (OnAfterPut != null)
                {
                    var e = new NKitRestApiPutEntityEventArgs(entityName, userName, DbContext, entityType, inputEntity);
                    OnAfterPut(this, e);
                }
                responseMessage = string.Format("{0} saved successfully.", entityName);
                LogWebResponse(requestName, responseMessage);
                return Ok(responseMessage);
            }
            finally
            {
                DisposeEntityContext();
            }
        }

        /// <summary>
        /// Inserts a single entity to a database table matching the entity name.
        /// This action is dynamic in that any database table can be specified as the entity name as long as it's been added to the DbContext as a DbSet.
        /// The request body can be in either JSON or XML format based on the configured SerializerType property set in the NKitWebApiControllerSettings section of the appsettings.json file.
        /// The request body can be text/plain, application/json or application/xml.
        /// Returns a text message simple message indicating that the entity has been saved. 
        /// The Content-Type of the is always text/plain.
        /// Called as such: "/{entityName}" with the JSON or XML contents as part of the request body.
        /// </summary>
        /// <param name="entityName">The name of the database table to save to.</param>
        /// <param name="serializedText">The JSON or XML representation of the entity (record) being inserted.</param>
        /// <returns>Returns a text message simple message indicating that the entity has been inserted. </returns>
        [HttpPost, Route("{entityName}")]
        [Consumes(MimeContentType.TEXT_PLAIN, MimeContentType.APPLICATION_JSON, MimeContentType.APPLICATION_XML)]
        [Produces(MimeContentType.TEXT_PLAIN)]
        public virtual IActionResult PostEntity(string entityName, [FromBody] string serializedText)
        {
            try
            {
                string requestName = $"{nameof(PostEntity)} : Entity Name = {entityName}";
                string responseMessage = null;
                ValidateRequestMethod(HttpVerb.POST);
                string userName = GetCurrentUserName();
                Type entityType = GetEntityType(entityName);
                object inputEntity = GetSerializer().DeserializeFromText(entityType, GetNKitSerializerModelTypes(), serializedText);
                LogWebRequest(requestName, serializedText);
                if (OnBeforePost != null)
                {
                    var e = new NKitRestApiPostEntityEventArgs(entityName, userName, DbContext, entityType, inputEntity);
                    OnBeforePost(this, e);
                    if (e.Cancel)
                    {
                        responseMessage = string.Format("{0} insert cancelled.", entityName);
                        LogWebResponse(requestName, responseMessage);
                        return Ok(responseMessage);
                    }
                }
                DbContext.Insert(entityType, inputEntity, null);
                if (OnAfterPost != null)
                {
                    OnAfterPost(this, new NKitRestApiPostEntityEventArgs(entityName, userName, DbContext, entityType, inputEntity));
                }
                responseMessage = string.Format("{0} inserted successfully.", entityName);
                LogWebResponse(requestName, responseMessage);
                return Ok(responseMessage);
            }
            finally
            {
                DisposeEntityContext();
            }
        }

        /// <summary>
        /// Deletes a single entity (table record) of the the specified entity name (table name) by searching for its surrogate key (single table key) supplied in the entityId parameter.
        /// This action is dynamic in that any database table can be specified as the entity name as long as it's been added to the DbContext as a DbSet.
        /// Returns a text message simple message indicating that the entity has been deleted. 
        /// The Content-Type of the is always text/plain.
        /// Called as such: "/{entityName}/{entityId}"
        /// </summary>
        /// <param name="entityName">The name of the database table to delete from.</param>
        /// <param name="entityId">The surrogate key (single table key) of the database record to be deleted.</param>
        /// <returns>Returns a text message simple message indicating that the entity has been deleted. </returns>
        [HttpDelete, Route("{entityName}/{entityId}")]
        [Produces(MimeContentType.TEXT_PLAIN)]
        public virtual IActionResult DeleteEntity(string entityName, string entityId)
        {
            try
            {
                string requestName = $"{nameof(DeleteEntity)} : Entity Name = {entityName} : Entity ID = {entityId}";
                string responseMessage = null;
                LogWebRequest(requestName, null);
                ValidateRequestMethod(HttpVerb.DELETE);
                string userName = GetCurrentUserName();
                Type entityType = GetEntityType(entityName);
                if (OnBeforeDelete != null)
                {
                    var e = new NKitRestApiDeleteEntityEventArgs(entityName, userName, DbContext, entityType, entityId);
                    OnBeforeDelete(this, e);
                    if (e.Cancel)
                    {
                        responseMessage = string.Format("{0} delete cancelled.", entityName);
                        LogWebResponse(requestName, responseMessage);
                        return Ok(responseMessage);
                    }
                }
                DbContext.DeleteBySurrogateKey(entityId, null, entityType);
                if (OnAfterDelete != null)
                {
                    OnAfterDelete(this, new NKitRestApiDeleteEntityEventArgs(entityName, userName, DbContext, entityType, entityId));
                }
                responseMessage = string.Format("{0} deleted successfully.", entityName);
                LogWebResponse(requestName, responseMessage);
                return Ok(responseMessage);
            }
            finally
            {
                DisposeEntityContext();
            }
        }

        /// <summary>
        /// Deletes all entities (table records) of the specified entity name (table name).
        /// This action is dynamic in that any database table can be specified as the entity name as long as it's been added to the DbContext as a DbSet.
        /// Returns a text message simple message indicating that the entities have been deleted. 
        /// N.B. It is not advised to use this action method for database tables that contain a large amount of records as the database command can timeout or lock up SQL server.
        /// For large database tables, a custom stored procedure should be developed to delete records in batches.
        /// The Content-Type of the is always text/plain.
        /// Called as such: "/{entityName}"
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        [HttpDelete, Route("{entityName}")]
        [Produces(MimeContentType.TEXT_PLAIN)]
        public virtual IActionResult DeleteAllEntities(string entityName)
        {
            try
            {
                string requestName = $"{nameof(DeleteAllEntities)} : Entity Name = {entityName}";
                string responseMessage = null;
                LogWebRequest(requestName, null);
                ValidateRequestMethod(HttpVerb.DELETE);
                string userName = GetCurrentUserName();
                Type entityType = GetEntityType(entityName);
                if (OnBeforeDeleteAll != null)
                {
                    var e = new NKitRestApiDeleteAllEntitiesEventArgs(entityName, userName, DbContext, entityType);
                    OnBeforeDeleteAll(this, e);
                    if (e.Cancel)
                    {
                        responseMessage = string.Format("{0} delete all cancelled.", entityName);
                        LogWebResponse(requestName, responseMessage);
                        return Ok(responseMessage);
                    }
                }
                DbContext.DeleteAll(entityType);
                if (OnAfterDeleteAll != null)
                {
                    OnAfterDeleteAll(this, new NKitRestApiDeleteAllEntitiesEventArgs(entityName, userName, DbContext, entityType));
                }
                responseMessage = string.Format("All {0} entities deleted successfully.", entityName);
                LogWebResponse(requestName, responseMessage);
                return Ok(responseMessage);
            }
            finally
            {
                DisposeEntityContext();
            }
        }

        #endregion //Actions
    }
}
