using System;
using Microsoft.AspNetCore.Http;

namespace ElmahCore.Mvc
{
    #region Imports

	#endregion

    class WebTemplateBase : RazorTemplateBase
    {
        public HttpContext Context { get; set; }
        public HttpResponse Response { get { return Context.Response; } }
        public HttpRequest Request { get { return Context.Request; } }
        public ElmahCore.ErrorLog ErrorLog { get; set; }

	    private string _elmahRoot = string.Empty;
	    public string ElmahRoot
	    {
		    get => Request.PathBase + _elmahRoot;
		    set => _elmahRoot = value;
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
                 : Mvc.Html.Encode(text).ToHtmlString();
        }

        public override void Write(object value)
        {
            if (value == null)
                return;
            base.Write(Mvc.Html.Encode(value).ToHtmlString());
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