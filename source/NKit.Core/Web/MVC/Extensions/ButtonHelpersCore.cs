namespace NKit.Web.MVC.Extensions
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web;
    using Microsoft.AspNetCore.Html;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;

    #endregion //Using Directives

    //http://volaresystems.com/blog/post/2012/09/16/Making-spiffy-buttons-with-CSS-and-MVC-Razor-helpers
    public static class ButtonHelpersCore
    {
        public static string GetHtmlFromTagBuilder(TagBuilder tagBuilder)
        {
            string result = string.Empty;
            using (StringWriter writer = new StringWriter())
            {
                tagBuilder.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
                result = writer.ToString();
            }
            return result;
        }

        public static HtmlString LinkButtonForSubmit(
            this IHtmlHelper helper,
            string buttonText,
            string javaScriptFunction,
            string controlId,
            HttpRequest request)
        {
            return LinkButtonForSubmit(helper, buttonText, javaScriptFunction, controlId, null, true, null, request);
        }

        public static HtmlString LinkButtonForSubmit(
            this IHtmlHelper helper,
            string buttonText,
            string javaScriptFunction,
            string controlId,
            string imageIconName,
            HttpRequest request)
        {
            return LinkButtonForSubmit(helper, buttonText, javaScriptFunction, controlId, null, true, imageIconName, request);
        }

        public static HtmlString LinkButtonForSubmit(
            this IHtmlHelper helper,
            string buttonText, 
            string javaScriptFunction,
            string controlId,
            string cssClass,
            bool includeImage,
            HttpRequest request)
        {
            return LinkButtonForSubmit(helper, buttonText, javaScriptFunction, controlId, cssClass, includeImage, null, request);
        }

        public static HtmlString LinkButtonForSubmit(
            this IHtmlHelper helper,
            string buttonText,
            string javaScriptFunction,
            string controlId,
            string cssClass,
            bool includeImage,
            string imageIconName,
            HttpRequest request)
        {
            string imageName = null;
            if (includeImage)
            {
                imageName = string.IsNullOrEmpty(imageIconName) ? "tick.png" : imageIconName;
            }
            TagBuilder imageTag = ImageTag(imageName, request);
            var anchorHtml = FormSubmitAnchorTag(imageTag, buttonText, javaScriptFunction, controlId, cssClass);
            HtmlString result = new HtmlString(anchorHtml);
            return result;
        }

        public static HtmlString LinkButtonForCancel(
            this IHtmlHelper helper,
            string buttonText,
            string onClickJavaScriptFunction,
            string controlId,
            HttpRequest request)
        {
            var areaName = AreaName(helper);
            string controllerName = ControllerName(helper);
            var imageTag = ImageTag("delete-2.png", request);
            var anchorHtml = AnchorTagWithImageAndText(
                areaName,
                controllerName,
                "Index",
                null,
                "button negative",
                imageTag,
                buttonText,
                onClickJavaScriptFunction,
                controlId);
            HtmlString result = new HtmlString(anchorHtml);
            return result;
        }

        private static TagBuilder ImageTag(string iconName, HttpRequest request)
        {
            var imageTag = new TagBuilder("img");
            if (iconName != null)
            {
                string relativePath = $"~/images/icons/{iconName}";
                string absolutePapth = ExtensionMethodsCore.GetAbsoluteFilePath(relativePath, request);
                imageTag.MergeAttribute("src", absolutePapth);
            }
            imageTag.MergeAttribute("width", "16");
            imageTag.MergeAttribute("height", "16");
            imageTag.MergeAttribute("alt", "");
            return imageTag;
        }

        private static string AreaName(IHtmlHelper helper)
        {
            var routeData = helper.ViewContext.RouteData;
            return routeData.DataTokens["area"] == null ? string.Empty : routeData.DataTokens["area"].ToString();
        }

        private static string ControllerName(IHtmlHelper helper)
        {
            var routeData = helper.ViewContext.RouteData;
            return routeData.Values["controller"].ToString();
        }

        private static string FormSubmitAnchorTag(TagBuilder imageTag, string anchorText, string onClickJavaScriptFunction, string controlId, string cssClass)
        {
            var anchorTag = new TagBuilder("a");
            if (!string.IsNullOrEmpty(controlId))
            {
                anchorTag.MergeAttribute("id", controlId);
            }
            if (!string.IsNullOrEmpty(onClickJavaScriptFunction))
            {
                anchorTag.MergeAttribute("onclick", onClickJavaScriptFunction);
            }
            else
            {
                anchorTag.MergeAttribute("href", "#");
            }
            if (cssClass == null)
            {
                anchorTag.AddCssClass("button positive");
            }
            else
            {
                anchorTag.AddCssClass(cssClass);
            }
            string imageTagHtml = GetHtmlFromTagBuilder(imageTag);
            anchorTag.InnerHtml.AppendHtml(imageTagHtml + anchorText);
            string result = GetHtmlFromTagBuilder(anchorTag);
            return result;
        }

        private static string AnchorTagWithImageAndText(
            string areaName,
            string controllerName,
            string actionName, int? id,
            string cssClass,
            TagBuilder imageTag,
            string anchorText,
            string onClickJavaScriptFunction,
            string controlId)
        {
            var anchorTag = new TagBuilder("a");
            if (!string.IsNullOrEmpty(controlId))
            {
                anchorTag.MergeAttribute("id", controlId);
            }
            if (!string.IsNullOrEmpty(onClickJavaScriptFunction))
            {
                anchorTag.MergeAttribute("onclick", onClickJavaScriptFunction);
            }
            else
            {
                if (areaName != string.Empty)
                {
                    anchorTag.MergeAttribute("href", id == null
                                                         ? string.Format("/{0}/{1}/{2}", areaName, controllerName, actionName)
                                                         : string.Format("/{0}/{1}/{2}/{3}", areaName, controllerName, actionName, id));
                }
                else
                {
                    anchorTag.MergeAttribute("href", id == null
                                                         ? string.Format("/{0}/{1}", controllerName, actionName)
                                                         : string.Format("/{0}/{1}/{2}", controllerName, actionName, id));
                }
            }
            anchorTag.AddCssClass(cssClass);
            anchorTag.InnerHtml.Clear();
            string imageTagHtml = GetHtmlFromTagBuilder(imageTag);
            anchorTag.InnerHtml.AppendHtml(imageTagHtml + anchorText);
            return GetHtmlFromTagBuilder(anchorTag);
        }
    }
}