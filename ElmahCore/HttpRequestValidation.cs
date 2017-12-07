#region License, Terms and Author(s)
//
// ELMAH - Error Logging Modules and Handlers for ASP.NET
// Copyright (c) 2004-9 Atif Aziz. All rights reserved.
//
//  Author(s):
//
//      Atif Aziz, http://www.raboof.com
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
using Microsoft.AspNetCore.Http;

namespace ElmahCore
{
    #region Imports

    #endregion

    static class HttpRequestValidation
    {
        /// <summary>
        /// Returns unvalidated collections if build targets .NET Framework
        /// 4.0 or later and if caller is hosted at run-time (based on value
        /// of <see cref="HostingEnvironment.IsHosted"/>) when targeting 
        /// .NET Framework 4.0 exclusively. In all other cases except when
        /// targeting .NET Framework 4.5, collections returned are validated 
        /// ones from <see cref="HttpRequestBase.Form"/> and 
        /// <see cref="QueryString"/> and therefore
        /// could raise <see cref="HttpRequestValidationException"/>.
        /// </summary>

        internal static T TryGetUnvalidatedCollections<T>(this HttpRequest request, 
            Func<IFormCollection, QueryString, IRequestCookieCollection, T> resultor)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (resultor == null) throw new ArgumentNullException(nameof(resultor));


            var qsfc = request;

            return resultor(string.IsNullOrEmpty(qsfc.ContentType) ? null : qsfc.Form,
                qsfc.QueryString, // ReSharper restore ConstantNullCoalescingCondition
                qsfc.Cookies);
        }

        
    }
}
