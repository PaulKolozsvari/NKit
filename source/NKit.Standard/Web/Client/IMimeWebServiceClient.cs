﻿namespace NKit.Web.Client
{
    #region Using Directives

    using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using NKit.Web.Client;

    #endregion //Using Directives

    public partial interface IMimeWebServiceClient
    {
        #region Methods

        void ConnectionTest(
            int timeout,
            out HttpStatusCode statusCode,
            out string statusDescription,
            bool wrapWebException,
            Dictionary<string, string> requestHeaders);

        T CallService<T>(
            string queryString,
            object requestPostObject,
            HttpVerb verb,
            out string rawOutput,
            bool serializePostObject,
            bool deserializeToDotNetObject,
            int timeout,
            out HttpStatusCode statusCode,
            out string statusDescription,
            bool wrapWebException,
            Dictionary<string, string> requestHeaders);

        T CallService<T>(
            string queryString,
            object requestPostObject,
            HttpVerb verb,
            out string rawOutput,
            bool serializePostObject,
            bool deserializeToDotNetObject,
            string postContentType,
            int timeout,
            string accept,
            out HttpStatusCode statusCode,
            out string statusDescription,
            bool wrapWebException,
            Dictionary<string, string> requestHeaders);

        object CallService(
            Type returnType,
            string queryString,
            object requestPostObject,
            HttpVerb verb,
            out string rawOutput,
            bool serializePostObject,
            bool deserializeToDotNetObject,
            int timeout,
            out HttpStatusCode statusCode,
            out string statusDescription,
            bool wrapWebException,
            Dictionary<string, string> requestHeaders);

        string PostBytes(
            string queryString,
            byte[] requestPostBytes,
            int timeout,
            string accept,
            out HttpStatusCode statusCode,
            out string statusDescription,
            bool wrapWebException);

        #endregion //Methods
    }
}
