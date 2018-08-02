using System;
using System.Collections.Generic;
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
	    public Func<HttpContext,bool> CheckPermissionAction { get; set; }
    }

}
