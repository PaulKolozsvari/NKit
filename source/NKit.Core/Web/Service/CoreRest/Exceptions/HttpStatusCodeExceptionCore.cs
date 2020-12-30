namespace NKit.Core.Web.Service.CoreRest.Exceptions
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text;
    using Newtonsoft.Json.Linq;
    using NKit.Web;

    #endregion //Using Directives

    public class HttpStatusCodeExceptionCore : Exception
    {
        #region Constructors

        public HttpStatusCodeExceptionCore(int statusCode, string message, string contentType) : base(message)
        {
            _statusCode = statusCode;
            _contentType = contentType ?? MimeContentType.TEXT_PLAIN;
        }

        public HttpStatusCodeExceptionCore(int statusCode) : this(statusCode, string.Empty, null)
        {
        }

        public HttpStatusCodeExceptionCore(int statusCode, string contentType) : this(statusCode, string.Empty, contentType)
        {
        }

        public HttpStatusCodeExceptionCore(HttpStatusCode httpStatusCode, string message) : this((int)httpStatusCode, message, null)
        {
        }

        public HttpStatusCodeExceptionCore(HttpStatusCode httpStatusCode, string message, string contentType) : this((int)httpStatusCode, message, contentType)
        {
        }

        public HttpStatusCodeExceptionCore(int statusCode, Exception exception) : this(statusCode, exception.Message, null)
        {
        }

        public HttpStatusCodeExceptionCore(int statusCode, Exception exception, string contentType) : this(statusCode, exception.Message, contentType)
        {
        }

        public HttpStatusCodeExceptionCore(HttpStatusCode httpStatusCode, Exception exception) : this((int)httpStatusCode, exception.Message, null)
        {
        }

        public HttpStatusCodeExceptionCore(HttpStatusCode httpStatusCode, Exception exception, string contentType) : this((int)httpStatusCode, exception.Message, contentType)
        {
        }

        public HttpStatusCodeExceptionCore(int statusCode, JObject errorObject) : this(statusCode, errorObject.ToString(), null)
        {
        }

        public HttpStatusCodeExceptionCore(int statusCode, JObject errorObject, string contentType) : this(statusCode, errorObject.ToString(), contentType)
        {
        }

        public HttpStatusCodeExceptionCore(HttpStatusCode httpStatusCode, JObject errorObject) : this((int)httpStatusCode, errorObject.ToString(), MimeContentType.APPLICATION_JSON)
        {
        }

        #endregion //Constructors

        #region Fields

        private int _statusCode;
        public string _contentType;

        #endregion //Fields

        #region Properties

        public int StatusCode
        {
            get { return _statusCode; }
        }

        public string ContentType
        {
            get { return _contentType; }
        }

        #endregion //Properties
    }
}
