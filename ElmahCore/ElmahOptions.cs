using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ElmahCore
{
    public class ElmahOptions 
    {
        public string Path { get; set; }
        public string LogPath { get; set; }
        public string FiltersConfig { get; set; } 

        public ICollection<IErrorFilter> Filters { get; set; } = new List<IErrorFilter>();
        public ICollection<IErrorNotifier> Notifiers { get; set; } = new List<IErrorNotifier>();
        public ErrorLog EventLog { get; set; }
        public string ConnectionString { get; set; }
        public Func<HttpContext, bool> OnPermissionCheck { get; set; } = context => true;
        public Func<HttpContext, Error, Task> OnError { get; set; } = (context, error) => Task.CompletedTask;
        public string ApplicationName { get; set; }

        public virtual bool PermissionCheck(HttpContext context) => OnPermissionCheck(context);
        public virtual Task Error(HttpContext context, Error error) => OnError(context, error);
    }

}
