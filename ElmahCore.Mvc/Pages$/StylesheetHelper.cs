#region License, Terms and Author(s)
//
// ELMAH - Error Logging Modules and Handlers for ASP.NET
// Copyright (c) 2004-9 Atif Aziz. All rights reserved.
//
//  Author(s):
//
//      James Driscoll, mailto:jamesdriscoll@btinternet.com
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ElmahCore.Mvc.MoreLinq;
using Microsoft.AspNetCore.Http;

namespace ElmahCore.Mvc
{
    #region Imports

	#endregion

    internal static class StyleSheetHelper
    {
        static string _styleSheetHash;
        static readonly ReadOnlyCollection<string> AllStyleSheetResourceNames = Array.AsReadOnly(new[] {"Bootstrap.css", "ErrorLog.css"});

        public static async Task LoadStyleSheets(HttpContext context, IEnumerable<string> resourceNames, string mediaType, Encoding responseEncoding, bool cacheResponse)
        {
            var response = context.Response;
            response.ContentType = mediaType;

            foreach (var resourceName in resourceNames)
                await ManifestResourceHelper.WriteResourceToStream(response.Body, typeof(StyleSheetHelper), resourceName);
        }
        public static string StyleSheetHash
        {
            get { return _styleSheetHash ?? (_styleSheetHash = CalculateHash().Result); }
        }

        public static IEnumerable<string> StyleSheetResourceNames
        {
            get { return AllStyleSheetResourceNames; }
        }

        private static async Task<string> CalculateHash()
        {
            var memoryStream = new MemoryStream();
            foreach (var resourceName in AllStyleSheetResourceNames)
                await ManifestResourceHelper.WriteResourceToStream(memoryStream, typeof(StyleSheetHelper), resourceName);

            return MD5.Create()
                      .ComputeHash(memoryStream)
                      .Select(b => b.ToString("x2"))
                      .ToDelimitedString(string.Empty);
        }
    }
}