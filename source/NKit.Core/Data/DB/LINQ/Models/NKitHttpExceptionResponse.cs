namespace NKit.Data.DB.LINQ.Models
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;
    using Microsoft.Extensions.Logging;
    using NKit.Utilities;
    using NKit.Utilities.Serialization;
    using NKit.Web;
    using NKit.Web.Service.RestApi.Exceptions;

    #endregion //Using Directives

    public class NKitHttpExceptionResponse : NKitBaseModel
    {
        #region Constructors

        public NKitHttpExceptionResponse()
        {
        }

        public NKitHttpExceptionResponse(NKitHttpStatusCodeException ex, bool includeExceptionStackTraceInErrorResponse)
        {
            NKitExceptionResponseId = Guid.NewGuid();
            HttpStatusCode = (int)ex.StatusCode;
            ExceptionMessage = ex.Message;
            if (ex.InnerException != null)
            {
                InnerExceptionMessage = ex.InnerException.Message;
            }
            if (includeExceptionStackTraceInErrorResponse)
            {
                ExceptionStackTrace = ex.StackTrace;
            }
            ExceptionOriginatingClassName = ex.OriginatingClassName;
            ExceptionOriginatingFunctionName = ex.OriginatingFunctionName;
            ContentType = ex.ContentType;
            EventId = ex.EventId.HasValue ? ex.EventId.Value.Id : 0;
            EventName = ex.EventId.HasValue ? ex.EventId.Value.Name : null;
            DateCreated = DateTime.Now;
        }

        public NKitHttpExceptionResponse(
            Exception ex, 
            int httpStatusCode,
            string contentType,
            Nullable<EventId> eventId,
            bool includeExceptionStackTraceInErrorResponse)
        {
            NKitExceptionResponseId = Guid.NewGuid();
            HttpStatusCode = httpStatusCode;
            ExceptionMessage = ex.Message;
            if (ex.InnerException != null)
            {
                InnerExceptionMessage = ex.InnerException.Message;
            }
            if (includeExceptionStackTraceInErrorResponse)
            {
                ExceptionStackTrace = ex.StackTrace;
            }
            ExceptionOriginatingClassName = ex.TargetSite != null ? ex.TargetSite.DeclaringType.FullName : string.Empty; ;
            ExceptionOriginatingFunctionName = ex.TargetSite != null ? ex.TargetSite.Name : string.Empty;
            ContentType = contentType;
            EventId = eventId.HasValue ? eventId.Value.Id : 0;
            EventName = eventId.HasValue ? eventId.Value.Name : null;
            DateCreated = DateTime.Now;
        }

        #endregion //Constructors

        #region Properties

        [Required]
        [Key]
        public Guid NKitExceptionResponseId { get; set; }

        [Required]
        public int HttpStatusCode { get; set; }

        [Required]
        [Column(TypeName = VARCHAR_MAX)]
        public string ExceptionMessage { get; set; }

        [Column(TypeName = VARCHAR_MAX)]
        public string InnerExceptionMessage { get; set; }

        [Column(TypeName = VARCHAR_MAX)]
        public string ExceptionStackTrace { get; set; }

        [Column(TypeName = VARCHAR_MAX)]
        public string ExceptionOriginatingClassName { get; set; }

        [Column(TypeName = VARCHAR_MAX)]
        public string ExceptionOriginatingFunctionName { get; set; }

        [Required]
        [Column(TypeName = VARCHAR_50)]
        public string ContentType { get; set; }

        [Required]
        public int EventId { get; set; }

        [Column(TypeName = VARCHAR_200)]
        public string EventName { get; set; }

        [Required]
        [Column(TypeName = DATE_TIME)]
        public DateTime DateCreated { get; set; }

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Based on the ContentType set on this object, it serializes this object using the appropriate Serializer.
        /// Returns JSON for content type application/json
        /// Returns XML for content type application/xml.
        /// Returns the plainTextResponse back for content type text/plain.
        /// Otherwise if none of the above content types have been specified, it uses the defaultSerializerType to serialize this object and sets the ContentType of this object to the defaultContentType.
        /// </summary>
        public string GetResponseText(string plainTextResponse, SerializerType defaultSerializerType, string defaultContentType)
        {
            switch (this.ContentType)
            {
                case MimeContentType.APPLICATION_JSON:
                    return GOC.Instance.GetSerializer(SerializerType.JSON).SerializeToText(this);
                case MimeContentType.APPLICATION_XML:
                    return GOC.Instance.GetSerializer(SerializerType.XML).SerializeToText(this);
                case MimeContentType.TEXT_PLAIN:
                    return plainTextResponse;
                default:
                    this.ContentType = defaultContentType;
                    return GOC.Instance.GetSerializer(defaultSerializerType).SerializeToText(this);
            }
        }

        #endregion //Methods
    }
}
