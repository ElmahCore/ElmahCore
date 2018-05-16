using ElmahCore;
using Microsoft.AspNetCore.Http;

namespace ElmahCore
{
    #region Imports

    using System;
    using System.Web;

    #endregion

    class WebTemplateBase : RazorTemplateBase
    {
        public HttpContext Context { get; set; }
        public HttpResponse Response { get { return Context.Response; } }
        public HttpRequest Request { get { return Context.Request; } }
        public ErrorLog ErrorLog { get; set; }
        private string elmahRoot = string.Empty;
        public string ElmahRoot
        {
            get { return Request.PathBase + elmahRoot; }
            set { elmahRoot = value; }
        }
        //public HttpServerUtilityBase Server { get { return Context.Server; } }


        public IHtmlString Html(string html)
        {
            return new HtmlString(html);
        }

        public string AttributeEncode(string text)
        {
            return string.IsNullOrEmpty(text)
                 ? string.Empty
                 : System.Net.WebUtility.HtmlEncode(text);
        }

        public string Encode(string text)
        {
            return string.IsNullOrEmpty(text)
                 ? string.Empty
                 : ElmahCore.Html.Encode(text).ToHtmlString();
        }

        public override void Write(object value)
        {
            if (value == null)
                return;
            base.Write(ElmahCore.Html.Encode(value).ToHtmlString());
        }

        public override object RenderBody()
        {
            return new HtmlString(base.RenderBody().ToString());
        }

        public override string TransformText()
        {
            if (Context == null)
                throw new InvalidOperationException("The Context property has not been initialzed with an instance.");
            return base.TransformText();
        }
    }
}