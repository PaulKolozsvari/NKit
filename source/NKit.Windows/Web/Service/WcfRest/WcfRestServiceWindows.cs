namespace NKit.Web.Service.WcfRest
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.ServiceModel;
    using System.ServiceModel.Web;
    using System.Text;
    using NKit.Data.DB.LINQ;
    using NKit.Utilities;
    using NKit.Utilities.Logging;
    using NKit.Utilities.Serialization;
    using NKit.Web.Client;
    using NKit.Web.Service.WcfRest.Events;
    using System.ServiceModel.Channels;
    using System.Threading;

    #endregion //Using Directives

    public partial class WcfRestServiceWindows : IWcfRestServiceWindows
    {
        #region Constructors

        public WcfRestServiceWindows() : this(false, true)
        {
        }

        public WcfRestServiceWindows(bool auditServiceCalls, bool handleExceptions)
        {
            _auditServiceCalls = auditServiceCalls;
            _handleExceptions = handleExceptions;
            if (!_serviceInstanceId.HasValue)
            {
                _serviceInstanceId = Guid.NewGuid();
            }
        }

        #endregion //Constructors

        #region Events

        public event OnBeforeGetEntitiesHandlerWindows OnBeforeGetEntities;
        public event OnAfterGetEntitiesHandlerWindows OnAfterGetEntities;

        public event OnBeforeGetEntityByIdHandlerWindows OnBeforeGetEntityById;
        public event OnAfterGetEntityByIdHandlerWindows OnAfterGetEntityById;

        public event OnBeforeGetEntitiesByFieldHandlerWindows OnBeforeGetEntitiesByField;
        public event OnAfterGetEntitiesByFieldHandlerWindows OnAfterGetEntitiesByField;

        public event OnBeforePutEntityHandlerWindows OnBeforePut;
        public event OnAfterPutEntityHandlerWindows OnAfterPut;

        public event OnBeforePostEntityHandlerWindows OnBeforePost;
        public event OnAfterPostEntityHandlerWindows OnAfterPost;

        public event OnBeforeDeleteEntityHandlerWindows OnBeforeDelete;
        public event OnAfterDeleteEntityHandlerWindows OnAfterDelete;

        protected bool _auditServiceCalls;
        protected Nullable<Guid> _serviceInstanceId;

        protected bool _handleExceptions;

        #endregion //Events

        #region Methods

        #region Utility Methods

        protected void AuditRequest(string requestName, string requestMessage)
        {
            if (!_auditServiceCalls)
            {
                return;
            }
            StringBuilder logMessage = new StringBuilder();
            logMessage.AppendLine($"Request Name: {requestName}");
            if (!string.IsNullOrEmpty(requestMessage))
            {
                logMessage.AppendLine($"Request Message:");
                logMessage.AppendLine(requestMessage);
            }
            AppendStatsToLogMessage(logMessage);
            GOCWindows.Instance.Logger.LogMessage(new LogMessage(logMessage.ToString(), LogMessageType.SuccessAudit, LoggingLevel.Maximum));
        }

        protected void AuditResponse(string requestName, string responseMessage)
        {
            if (!_auditServiceCalls)
            {
                return;
            }
            StringBuilder logMessage = new StringBuilder();
            logMessage.AppendLine($"Response Message: {requestName}");
            if (!string.IsNullOrEmpty(responseMessage))
            {
                logMessage.AppendLine($"Request Message:");
                logMessage.AppendLine(responseMessage);
            }
            AppendStatsToLogMessage(logMessage);
            GOCWindows.Instance.Logger.LogMessage(new LogMessage(logMessage.ToString(), LogMessageType.SuccessAudit, LoggingLevel.Maximum));
        }

        private void AppendStatsToLogMessage(StringBuilder logMessage)
        {
            ThreadHelper.GetCurrentThreadCount(out int workerThreadsRunning, out int completionPortThreadsRunning);
            int totalThreadsRunning = ThreadHelper.GetTotalThreadsRunningCountInCurrentProcess();
            logMessage.AppendLine($"Request URI: {GetCurrentRequestUri()}");
            logMessage.AppendLine($"Request Verb: {WebOperationContext.Current.IncomingRequest.Method}");
            logMessage.AppendLine($"User Name: {GetUserName()}");
            logMessage.AppendLine($"Client IP Address: {GetCurrentRequestClientIpAddress()}");
            logMessage.AppendLine($"Service Instance ID: {_serviceInstanceId}");
            logMessage.AppendLine($"Worker Threads Running: {workerThreadsRunning}");
            logMessage.AppendLine($"Completion Port Threads Running: {completionPortThreadsRunning}");
            logMessage.AppendLine($"Total Threads Running: {totalThreadsRunning}");
            logMessage.AppendLine($"Current Thread ID: {Thread.CurrentThread.ManagedThreadId}");
        }

        public string GetCurrentRequestUri()
        {
            IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
            string result = request.UriTemplateMatch.RequestUri.OriginalString;
            return result;
        }

        public string GetAllHeadersFullString()
        {
            IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
            return request.Headers.ToString();
        }

        public string GetAllHeadersFormatted()
        {
            IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
            StringBuilder result = new StringBuilder();
            foreach (var key in request.Headers.AllKeys)
            {
                result.AppendLine(string.Format("{0}={1}", key, request.Headers[key]));
            }
            return result.ToString();
        }

        public string GetHeader(string key, bool throwExceptionOnNotFound)
        {
            IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
            string result = string.Empty;
            if (request != null && request.Headers.HasKeys())
            {
                result = request.Headers.Get(key);
            }
            if (result == null && throwExceptionOnNotFound)
            {
                throw new NullReferenceException(string.Format("Could not find HTTP Header with key {0}.", key));
            }
            return result;
        }

        protected virtual void UpdateHttpStatusOnException(Exception ex)
        {
            WebOperationContext context = WebOperationContext.Current;
            if (context.OutgoingResponse.StatusCode == HttpStatusCode.OK ||
                context.OutgoingResponse.StatusCode == HttpStatusCode.Created)
            {
                context.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.InternalServerError;
            }
            /*A status description with escape sequences causes the server to not respond to the request 
            causing the client to not get a response therefore not know what the exception was.*/
            string errorMessage = ex.Message.Replace("\r", string.Empty);
            errorMessage = errorMessage.Replace("\n", string.Empty);
            errorMessage = errorMessage.Replace("\t", string.Empty);
            context.OutgoingResponse.StatusDescription = errorMessage;
        }

        protected virtual void UpdateHttpStatusOnExceptionWithStackTrace(Exception ex)
        {
            WebOperationContext context = WebOperationContext.Current;
            if (context.OutgoingResponse.StatusCode == HttpStatusCode.OK ||
                context.OutgoingResponse.StatusCode == HttpStatusCode.Created)
            {
                context.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.InternalServerError;
            }
            /*A status description with escape sequences causes the server to not respond to the request 
            causing the client to not get a response therefore not know what the exception was.*/
            string errorMessage = ex.Message.Replace("\r", string.Empty);
            errorMessage += "Stack Trace: ";
            errorMessage += ex.StackTrace.Replace("\r", string.Empty);
            errorMessage = errorMessage.Replace("\n", string.Empty);
            errorMessage = errorMessage.Replace("\t", string.Empty);
            context.OutgoingResponse.StatusDescription = errorMessage;
        }

        protected virtual void ValidateRequestMethod(HttpVerb verb)
        {
            ValidateRequestMethod(verb.ToString());
        }

        protected virtual void ValidateRequestMethod(string verb)
        {
            if (WebOperationContext.Current.IncomingRequest.Method != verb)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.MethodNotAllowed;
                throw new UserThrownException(
                    string.Format(
                    "Unexpected Method of {0} on incoming POST Request {1}.",
                    WebOperationContext.Current.IncomingRequest.Method,
                    WebOperationContext.Current.IncomingRequest.UriTemplateMatch.RequestUri.ToString()),
                    LoggingLevel.Normal);
            }
        }

        protected virtual Stream GetStreamFromObject(object obj)
        {
            return GetStreamFromObject(obj, out string serializedText);
        }

        protected virtual Stream GetStreamFromObject(object obj, out string serializedText)
        {
            serializedText = GOCWindows.Instance.JsonSerializer.SerializeToText(obj);
            return StreamHelper.GetStreamFromString(serializedText, GOCWindows.Instance.Encoding);
        }

        protected virtual Stream GetStreamFromObject(object obj, Type[] extraTypes)
        {
            return GetStreamFromObject(obj, extraTypes, out string serializedText);
        }

        protected virtual Stream GetStreamFromObject(object obj, Type[] extraTypes, out string serializedText)
        {
            serializedText = GOCWindows.Instance.JsonSerializer.SerializeToText(obj, extraTypes);
            return StreamHelper.GetStreamFromString(serializedText, GOCWindows.Instance.Encoding);
        }

        protected virtual Stream GetStreamFromObject(object obj, ISerializer serializer)
        {
            return StreamHelper.GetStreamFromString(serializer.SerializeToText(obj), GOCWindows.Instance.Encoding);
        }

        protected virtual Stream GetStreamFromObject(object obj, ISerializer serializer, out string serializedText)
        {
            serializedText = serializer.SerializeToText(obj);
            return StreamHelper.GetStreamFromString(serializedText, GOCWindows.Instance.Encoding);
        }

        protected virtual Stream GetStreamFromObject(object obj, ISerializer serializer, Type[] extraTypes)
        {
            return StreamHelper.GetStreamFromString(serializer.SerializeToText(obj, extraTypes), GOCWindows.Instance.Encoding);
        }

        protected virtual Stream GetStreamFromObject(object obj, ISerializer serializer, Type[] extraTypes, out string serializedText)
        {
            serializedText = serializer.SerializeToText(obj, extraTypes);
            return StreamHelper.GetStreamFromString(serializedText, GOCWindows.Instance.Encoding);
        }

        protected virtual E GetObjectFromStream<E>(Stream stream, ISerializer serializer) where E : class
        {
            return (E)GetObjectFromStream(typeof(E), stream, serializer, out string serializedText);
        }

        protected virtual E GetObjectFromStream<E>(Stream stream, ISerializer serializer, out string serializedText) where E : class
        {
            return (E)GetObjectFromStream(typeof(E), stream, serializer, out serializedText);
        }

        protected virtual object GetObjectFromStream(Type entityType, Stream stream, ISerializer serializer)
        {
            return GetObjectFromStream(entityType, stream, serializer, out string serializedText);
        }

        protected virtual object GetObjectFromStream(Type entityType, Stream stream, ISerializer serializer, out string serializedText)
        {
            serializedText = StreamHelper.GetStringFromStream(stream, GOCWindows.Instance.Encoding);
            object result = serializer.DeserializeFromText(entityType, serializedText);
            if (result == null)
            {
                throw new Exception(string.Format("The following text could not be deserialized to a {0} : {1}", entityType.FullName, serializedText));
            }
            return result;
        }

        protected virtual void GetUserDetails(LinqEntityContextWindows context, out Nullable<Guid> userId, out string userName)
        {
            userId = null;
            userName = null;
            if (ServiceSecurityContext.Current != null && !string.IsNullOrEmpty(ServiceSecurityContext.Current.WindowsIdentity.Name))
            {
                if (context != null && context.UserLinqToSqlType != null)
                {
                    userId = context.GetUserId(ServiceSecurityContext.Current.WindowsIdentity.Name);
                }
                userName = ServiceSecurityContext.Current.WindowsIdentity.Name;
            }
        }

        protected virtual string GetUserName()
        {
            string result = string.Empty;
            if (ServiceSecurityContext.Current != null && !string.IsNullOrEmpty(ServiceSecurityContext.Current.WindowsIdentity.Name))
            {
                result = ServiceSecurityContext.Current.WindowsIdentity.Name;
            }
            return result;
        }

        protected virtual Type GetEntityType(string entityName)
        {
            Type result = AssemblyReader.FindType(
                GOCWindows.Instance.LinqToClassesAssembly,
                GOCWindows.Instance.LinqToSQLClassesNamespace,
                entityName,
                false);
            if (result == null)
            {
                throw new NullReferenceException(string.Format("Could not find entity with name {0}.", entityName));
            }
            return result;
        }

        public static LinqEntityContextWindows GetEntityContext()
        {
            return new LinqEntityContextWindows(
                GOCWindows.Instance.GetNewLinqToSqlDataContext(),
                GOCWindows.Instance.GetByTypeName<LinqFunnelSettings>(),
                false,
                GOCWindows.Instance.UserLinqToSqlType,
                GOCWindows.Instance.ServerActionLinqToSqlType,
                GOCWindows.Instance.ServerErrorLinqToSqlType,
                GOCWindows.Instance.DatabaseTransactionScopeOption,
                new System.Transactions.TransactionOptions()
                {
                    IsolationLevel = GOCWindows.Instance.DatabaseTransactionIsolationLevel,
                    Timeout = new TimeSpan(0, 0, GOCWindows.Instance.DatabaseTransactionTimeoutSeconds)
                },
                GOCWindows.Instance.DatabaseTransactionDeadlockRetryAttempts,
                GOCWindows.Instance.DatabaseTransactionDeadlockRetryWaitPeriod);
        }

        public static void DisposeEntityContext(LinqEntityContextWindows context)
        {
            if (context != null)
            {
                context.Dispose();
            }
        }

        protected string GetCurrentRequestClientIpAddress()
        {
            MessageProperties props = OperationContext.Current.IncomingMessageProperties;
            RemoteEndpointMessageProperty endpointProperty = props[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
            string result = null;
            if (endpointProperty != null)
            {
                result = endpointProperty.Address;
            }
            return result;
        }

        #endregion //Utility Methods

        #region REST Service Methods

        public virtual Stream AllURIs()
        {
            try
            {
                string requestName = $"{nameof(AllURIs)}";
                AuditRequest(requestName, null);
                string allHeadersFullString = GetAllHeadersFullString();
                StringBuilder message = new StringBuilder();
                string applicationName = string.IsNullOrEmpty(GOCWindows.Instance.ApplicationName) ? "REST Web Service" : GOCWindows.Instance.ApplicationName;
                message.AppendLine(applicationName);
                message.AppendLine();
                message.AppendLine("HTTP Headers:");
                message.AppendLine();
                message.AppendLine(allHeadersFullString);
                message.AppendLine();
                string responseMessage = message.ToString();
                Stream result = StreamHelper.GetStreamFromString(responseMessage, GOCWindows.Instance.Encoding);
                AuditResponse(requestName, responseMessage);
                return result;
            }
            catch (Exception ex)
            {
                if (_handleExceptions)
                {
                    ExceptionHandlerWindows.HandleException(ex, null);
                }
                UpdateHttpStatusOnException(ex);
                throw ex;
            }
        }

        public virtual Stream GetEntities(string entityName)
        {
            LinqEntityContextWindows context = null;
            try
            {
                string requestName = $"{nameof(GetEntities)} : Entity Name = {entityName}";
                AuditRequest(requestName, null);
                ValidateRequestMethod(HttpVerb.GET);
                context = GetEntityContext();
                Nullable<Guid> userId = null;
                string userName = null;
                GetUserDetails(context, out userId, out userName);
                Type entityType = GetEntityType(entityName);
                if (OnBeforeGetEntities != null)
                {
                    OnBeforeGetEntities(this, new RestServiceGetEntitiesEventArgsWindows(
                        entityName, userId, userName, context, entityType, null));
                }
                List<object> outputEntities = context.GetAllEntities(
                    entityType,
                    false,
                    userId,
                    userName).Contents;
                if (OnAfterGetEntities != null)
                {
                    OnAfterGetEntities(this, new RestServiceGetEntitiesEventArgsWindows(
                        entityName, userId, userName, context, entityType, outputEntities));
                }
                Stream result = GetStreamFromObject(outputEntities, out string serializedText);
                AuditResponse(requestName, serializedText);
                return result;
            }
            catch (Exception ex)
            {
                if (_handleExceptions)
                {
                    ExceptionHandlerWindows.HandleException(ex, null);
                }
                UpdateHttpStatusOnException(ex);
                throw ex;
            }
            finally
            {
                DisposeEntityContext(context);
            }
        }

        public virtual Stream GetEntityById(string entityName, string entityId)
        {
            LinqEntityContextWindows context = null;
            try
            {
                string requestName = $"{nameof(GetEntityById)} : Entity Name = {entityName} : Entity ID = {entityId}";
                AuditRequest(requestName, null);
                ValidateRequestMethod(HttpVerb.GET);
                context = GetEntityContext();
                Nullable<Guid> userId = null;
                string userName = null;
                GetUserDetails(context, out userId, out userName);
                Type entityType = GetEntityType(entityName);
                if (OnBeforeGetEntityById != null)
                {
                    OnBeforeGetEntityById(this, new RestServiceGetEntityByIdEventArgsWindows(
                        entityName, userId, userName, context, entityType, entityId, null));
                }
                object outputEntity = context.GetEntityBySurrogateKey(
                    entityType,
                    entityId,
                    false,
                    userId,
                    userName).Contents;
                if (OnAfterGetEntityById != null)
                {
                    OnAfterGetEntityById(this, new RestServiceGetEntityByIdEventArgsWindows(
                        entityName, userId, userName, context, entityType, entityId, outputEntity));
                }
                Stream result = GetStreamFromObject(outputEntity, out string serializedText);
                AuditResponse(requestName, serializedText);
                return result;
            }
            catch (Exception ex)
            {
                if (_handleExceptions)
                {
                    ExceptionHandlerWindows.HandleException(ex, null);
                }
                UpdateHttpStatusOnException(ex);
                throw ex;
            }
            finally
            {
                DisposeEntityContext(context);
            }
        }

        public virtual Stream GetEntitiesByField(string entityName, string fieldName, string fieldValue)
        {
            LinqEntityContextWindows context = null;
            try
            {
                string requestName = $"{nameof(GetEntitiesByField)} : Entity Name = {entityName} : Field Name = {fieldName} : Field Value = {fieldValue}";
                AuditRequest(requestName, null);
                ValidateRequestMethod(HttpVerb.GET);
                context = GetEntityContext();
                Nullable<Guid> userId = null;
                string userName = null;
                GetUserDetails(context, out userId, out userName);
                Type entityType = GetEntityType(entityName);
                if (OnBeforeGetEntitiesByField != null)
                {
                    OnBeforeGetEntitiesByField(this, new RestServiceGetEntitiesByFieldEventArgsWindows(
                        entityName, userId, userName, context, entityType, fieldName, fieldValue, null));
                }
                List<object> outputEntities = context.GetEntitiesByField(
                    entityType,
                    fieldName,
                    fieldValue,
                    false,
                    userId,
                    userName).Contents;
                if(OnAfterGetEntitiesByField != null)
                {
                    OnAfterGetEntitiesByField(this, new RestServiceGetEntitiesByFieldEventArgsWindows(
                        entityName, userId, userName, context, entityType, fieldName, fieldValue, outputEntities));
                }
                Stream result = GetStreamFromObject(outputEntities, out string serializedText);
                AuditResponse(requestName, serializedText);
                return result;
            }
            catch (Exception ex)
            {
                if (_handleExceptions)
                {
                    ExceptionHandlerWindows.HandleException(ex, null);
                }
                UpdateHttpStatusOnException(ex);
                throw ex;
            }
            finally
            {
                DisposeEntityContext(context);
            }
        }

        public virtual Stream PutEntity(string entityName, Stream inputStream)
        {
            LinqEntityContextWindows context = null;
            Stream result = null;
            string responseMessage = null;
            try
            {
                string requestName = $"{nameof(PutEntity)} : Entity Name : {entityName}";
                ValidateRequestMethod(HttpVerb.PUT);
                context = GetEntityContext();
                Nullable<Guid> userId = null;
                string userName = null;
                Type entityType = GetEntityType(entityName);
                GetUserDetails(context, out userId, out userName);
                object inputEntity = GetObjectFromStream(entityType, inputStream, GOCWindows.Instance.JsonSerializer, out string serializedText);
                AuditRequest(requestName, serializedText);
                if (OnBeforePut != null)
                {
                    RestServicePutEntityEventArgsWindows e = new RestServicePutEntityEventArgsWindows(
                        entityName, userId, userName, context, entityType, inputEntity);
                    OnBeforePut(this, e);
                    if (e.Cancel)
                    {
                        responseMessage = string.Format("{0} saving cancelled.", entityName);
                        result = StreamHelper.GetStreamFromString(responseMessage, GOCWindows.Instance.Encoding);
                        AuditResponse(requestName, responseMessage);
                        return result;
                    }
                }
                context.Save(
                    entityType,
                    new List<object>() { inputEntity },
                    userId,
                    userName,
                    false);
                if (OnAfterPut != null)
                {
                    OnAfterPut(this, new RestServicePutEntityEventArgsWindows(
                        entityName, userId, userName, context, entityType, inputEntity));
                }
                responseMessage = string.Format("{0} saved successfully.", entityName);
                result = StreamHelper.GetStreamFromString(responseMessage, GOCWindows.Instance.Encoding);
                AuditResponse(requestName, responseMessage);
                return result;
            }
            catch (Exception ex)
            {
                if (_handleExceptions)
                {
                    ExceptionHandlerWindows.HandleException(ex, null);
                }
                UpdateHttpStatusOnException(ex);
                throw ex;
            }
            finally
            {
                DisposeEntityContext(context);
            }
        }

        public virtual Stream PostEntity(string entityName, Stream inputStream)
        {
            LinqEntityContextWindows context = null;
            string responseMessage = null;
            Stream result = null;
            try
            {
                string requestName = $"{nameof(PostEntity)} : Entity Name = {entityName}";
                ValidateRequestMethod(HttpVerb.POST);
                context = GetEntityContext();
                Nullable<Guid> userId = null;
                string userName = null;
                Type entityType = GetEntityType(entityName);
                GetUserDetails(context, out userId, out userName);
                object inputEntity = GetObjectFromStream(entityType, inputStream, GOCWindows.Instance.JsonSerializer, out string serializedText);
                AuditRequest(requestName, serializedText);
                if (OnBeforePost != null)
                {
                    RestServicePostEntityEventArgsWindows e = new RestServicePostEntityEventArgsWindows(
                        entityName, userId, userName, context, entityType, inputEntity);
                    OnBeforePost(this, e);
                    if (e.Cancel)
                    {
                        responseMessage = string.Format("{0} inserting cancelled.", entityName);
                        result = StreamHelper.GetStreamFromString(responseMessage, GOCWindows.Instance.Encoding);
                        AuditResponse(requestName, responseMessage);
                        return result;
                    }
                }
                context.Insert(
                    entityType,
                    new List<object>() { inputEntity },
                    userId,
                    userName,
                    false);
                if (OnAfterPost != null)
                {
                    OnAfterPost(this, new RestServicePostEntityEventArgsWindows(
                        entityName, userId, userName, context, entityType, inputEntity));
                }
                responseMessage = string.Format("{0} insert successfully.", entityName);
                result = StreamHelper.GetStreamFromString(responseMessage, GOCWindows.Instance.Encoding);
                AuditResponse(requestName, responseMessage);
                return result;
            }
            catch (Exception ex)
            {
                if (_handleExceptions)
                {
                    ExceptionHandlerWindows.HandleException(ex, null);
                }
                UpdateHttpStatusOnException(ex);
                throw ex;
            }
            finally
            {
                DisposeEntityContext(context);
            }
        }

        public virtual Stream DeleteEntity(string entityName, string entityId)
        {
            LinqEntityContextWindows context = null;
            try
            {
                string requestName = $"{nameof(DeleteEntity)} : Entity Name = {entityName} : Entity ID = {entityId}";
                AuditRequest(requestName, null);
                ValidateRequestMethod(HttpVerb.DELETE);
                context = GetEntityContext();
                Nullable<Guid> userId = null;
                string userName = null;
                GetUserDetails(context, out userId, out userName);
                Type entityType = GetEntityType(entityName);
                if (OnBeforeDelete != null)
                {
                    OnBeforeDelete(this, new RestServiceDeleteEntityEventArgsWindows(
                        entityName, userId, userName, context, entityType, entityId));
                }
                context.DeleteBySurrogateKey(
                    entityType,
                    new List<object>() { entityId },
                    userId,
                    userName);
                if (OnAfterDelete != null)
                {
                    OnAfterDelete(this, new RestServiceDeleteEntityEventArgsWindows(
                        entityName, userId, userName, context, entityType, entityId));
                }
                string responseMessage = string.Format("{0} deleted successfully.", entityName);
                Stream result = StreamHelper.GetStreamFromString(responseMessage, GOCWindows.Instance.Encoding);
                AuditResponse(requestName, responseMessage);
                return result;
            }
            catch (Exception ex)
            {
                if (_handleExceptions)
                {
                    ExceptionHandlerWindows.HandleException(ex, null);
                }
                UpdateHttpStatusOnException(ex);
                throw ex;
            }
            finally
            {
                DisposeEntityContext(context);
            }
        }

        //http://blogs.msdn.com/b/webapps/archive/2012/09/06/wcf-chunking.aspx
        public virtual Stream FileUpload(string fileName, Stream inputStream)
        {
            try
            {
                string requestName = $"{nameof(FileUpload)} : File Name = {fileName}";
                AuditRequest(requestName, null);
                ValidateRequestMethod(HttpVerb.POST);
                byte[] fileBytes = StreamHelper.GetBytesFromStream(inputStream);
                FileStream fs = null;
                lock (FileUploadSessions.Instance.FileStreams)
                {
                    FileUploadSessions.Instance.FileStreams.TryGetValue(fileName, out fs);
                    if (fs == null)
                    {
                        fs = File.Open(fileName, FileMode.Create, FileAccess.ReadWrite);
                        FileUploadSessions.Instance.FileStreams.Add(fileName, fs);
                    }
                    fs.Write(fileBytes, 0, fileBytes.Length);
                    fs.Flush();
                }
                string responseMessage = string.Format("{0} bytes written to {1}", fileBytes.Length, fileName);
                Stream result = StreamHelper.GetStreamFromString(
                    responseMessage,
                    GOCWindows.Instance.Encoding);
                AuditResponse(requestName, responseMessage);
                return result;
            }
            catch (Exception ex)
            {
                FileUploadCompleted(fileName);
                if (_handleExceptions)
                {
                    ExceptionHandlerWindows.HandleException(ex, null);
                }
                UpdateHttpStatusOnException(ex);
                throw ex;
            }
        }

        public virtual Stream FileUploadCompleted(string fileName)
        {
            try
            {
                string requestName = $"{nameof(FileUploadCompleted)} : File Name = {fileName}";
                AuditRequest(requestName, null);
                ValidateRequestMethod(HttpVerb.POST);
                FileStream fs = null;
                lock (FileUploadSessions.Instance.FileStreams)
                {
                    FileUploadSessions.Instance.FileStreams.TryGetValue(fileName, out fs);
                    if (fs == null)
                    {
                        throw new Exception(string.Format("Could not find {0} to closse for file {1}", typeof(FileStream).Name, fileName));
                    }
                    fs.Close();
                    fs.Dispose();
                    FileUploadSessions.Instance.FileStreams.Remove(fileName);
                }
                string responseMessage = string.Format("{0} for {1} closed.", typeof(FileStream).Name, fileName);
                Stream result = StreamHelper.GetStreamFromString(responseMessage, GOCWindows.Instance.Encoding);
                AuditResponse(requestName, responseMessage);
                return result;
            }
            catch (Exception ex)
            {
                if (_handleExceptions)
                {
                    ExceptionHandlerWindows.HandleException(ex, null);
                }
                UpdateHttpStatusOnException(ex);
                throw;
            }
        }

        public virtual Stream FileDownload(string fileName)
        {
            try
            {
                string requestName = $"{nameof(FileDownload)}: File Name = {fileName}";
                AuditRequest(requestName, null);
                FileSystemHelper.ValidateDirectoryExists(fileName);
                Stream result = File.OpenRead(fileName);
                AuditResponse(requestName, null);
                return result;
            }
            catch (Exception ex)
            {
                if (_handleExceptions)
                {
                    ExceptionHandlerWindows.HandleException(ex, null);
                }
                UpdateHttpStatusOnException(ex);
                throw;
            }
        }

        #endregion //REST Service Methods

        #endregion //Methods
    }
}
