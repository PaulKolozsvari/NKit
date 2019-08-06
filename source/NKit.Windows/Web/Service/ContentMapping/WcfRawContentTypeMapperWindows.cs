namespace NKit.Web.Service.ContentMapping
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel.Channels;
    using System.Text;
    using System.Threading.Tasks;

    #endregion //Using Directives

    public class WcfRawContentTypeMapperWindows : WebContentTypeMapper
    {
        #region Methods

        public override WebContentFormat GetMessageFormatForContentType(string contentType)
        {
            if (contentType.Contains("text/xml") || 
                contentType.Contains("application/xml") ||
                contentType.Contains("application/json"))
            {
                return WebContentFormat.Raw;
            }
            else
            {
                return WebContentFormat.Default;
            }
        }

        #endregion //Methods
    }
}
