namespace NKit.Web.Service.RestApi.Exceptions
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    using NKit.Web;

    #endregion //Using Directives

    public class NKitHttpStatusCodeException : Exception
    {
        #region Constructors

        #region Constructors with plain info

        public NKitHttpStatusCodeException(HttpStatusCode statusCode, string message, string contentType, Nullable<EventId> eventId) : base(message)
        {
            _statusCode = statusCode;
            _contentType = contentType ?? MimeContentType.TEXT_PLAIN;
            _eventId = eventId;
        }

        public NKitHttpStatusCodeException(HttpStatusCode httpStatusCode, string message) : this(httpStatusCode, message, null, null)
        {
        }

        public NKitHttpStatusCodeException(HttpStatusCode httpStatusCode, string message, string contentType) : this(httpStatusCode, message, contentType, null)
        {
        }

        #endregion //Constructors with plain info

        #region Constructors with Exception

        public NKitHttpStatusCodeException(HttpStatusCode httpStatusCode, Exception exception) : this(httpStatusCode, exception.Message, null, null)
        {
        }

        public NKitHttpStatusCodeException(HttpStatusCode httpStatusCode, Exception exception, EventId eventId) : this(httpStatusCode, exception.Message, null, eventId)
        {
        }

        public NKitHttpStatusCodeException(HttpStatusCode httpStatusCode, Exception exception, string contentType) : this(httpStatusCode, exception.Message, contentType, null)
        {
        }

        public NKitHttpStatusCodeException(HttpStatusCode httpStatusCode, Exception exception, string contentType, EventId eventId) : this(httpStatusCode, exception.Message, contentType, eventId)
        {
        }

        #endregion //Constructors with Exception

        #region Constructors with JObject

        public NKitHttpStatusCodeException(HttpStatusCode httpStatusCode, JObject errorObject) : this(httpStatusCode, errorObject.ToString(), MimeContentType.APPLICATION_JSON, null)
        {
        }

        public NKitHttpStatusCodeException(HttpStatusCode httpStatusCode, JObject errorObject, EventId eventId) : this(httpStatusCode, errorObject.ToString(), MimeContentType.APPLICATION_JSON, eventId)
        {
        }

        #endregion //Constructors with JObject

        #endregion //Constructors

        #region Fields

        protected HttpStatusCode _statusCode;
        protected string _contentType;
        protected string _source;
        protected Nullable<EventId> _eventId;

        #endregion //Fields

        #region Properties

        public HttpStatusCode StatusCode
        {
            get { return _statusCode; }
        }

        public string ContentType
        {
            get { return _contentType; }
        }

        public Nullable<EventId> EventId
        {
            get { return _eventId; }
        }

        public string OriginatingClassName
        {
            get{ return this.TargetSite != null ? this.TargetSite.DeclaringType.FullName : string.Empty; }
        }

        public string OriginatingFunctionName
        {
            get { return this.TargetSite != null ? this.TargetSite.Name : string.Empty; }
        }

        #endregion //Properties
    }
}
