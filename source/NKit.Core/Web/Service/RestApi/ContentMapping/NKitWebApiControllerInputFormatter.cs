namespace NKit.Web.Service.RestApi.ContentMapping
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using NKit.Data;
    using NKit.Web;

    #endregion //Using Directives

    /// <summary>
    /// Configures controllers to allow receiving of plain text or any of the media types specified in the constructor of this class.
    /// https://peterdaugaardrasmussen.com/2020/02/29/asp-net-core-how-to-make-a-controller-endpoint-that-accepts-text-plain/
    /// </summary>
    public class NKitWebApiControllerInputFormatter : InputFormatter
    {
        #region Constructors

        public NKitWebApiControllerInputFormatter() : this(new string[]
            {
                MimeContentType.APPLICATION_XML,
                MimeContentType.APPLICATION_JSON,
                MimeContentType.TEXT_XML,
                MimeContentType.TEXT_PLAIN,
                MimeContentType.TEXT_HTML,
                MimeContentType.BINARY,
                MimeContentType.FORM_DATA,
                MimeContentType.ANY
            })
        {
        }

        public NKitWebApiControllerInputFormatter(string[] supportedMediaTypes)
        {
            _supportedMediaTypes = supportedMediaTypes;
            if (_supportedMediaTypes != null)
            {
                _supportedMediaTypes.ToList().ForEach(p => SupportedMediaTypes.Add(p));
            }
        }

        #endregion //Constructors

        #region Fields

        private string[] _supportedMediaTypes; 

        #endregion //Fields

        #region Methods

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            var request = context.HttpContext.Request;
            using (var reader = new StreamReader(request.Body))
            {
                var content = await reader.ReadToEndAsync();
                return await InputFormatterResult.SuccessAsync(content);
            }
        }

        public override bool CanRead(InputFormatterContext context)
        {
            var contentType = context.HttpContext.Request.ContentType;
            if (_supportedMediaTypes == null)
            {
                return false;
            }
            return _supportedMediaTypes.Any(p => contentType.StartsWith(p));
        }

        #endregion //Methods
    }
}
