namespace ElmahCore.DemoCore6
{
    public class CmsErrorLogFilter : IErrorFilter
    {
        public void OnErrorModuleFiltering(object sender, ExceptionFilterEventArgs args)
        {
            if (args.Exception.GetBaseException() is FileNotFoundException)
            {
                args.Dismiss();
            }

            if (args.Context is HttpContext httpContext)
            {
                if (httpContext.Response.StatusCode == 404)
                {
                    args.Dismiss();
                }
            }
        }
    }
}
