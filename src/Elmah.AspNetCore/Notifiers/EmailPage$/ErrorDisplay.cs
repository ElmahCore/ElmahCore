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

//[assembly: Elmah.Scc("$Id: ErrorDisplay.cs 607 2009-05-27 23:47:10Z azizatif $")]

using System;
using System.Globalization;

namespace Elmah.AspNetCore.Notifiers;

/// <summary>
///     Provides miscellaneous formatting methods for
/// </summary>
internal static class ErrorDisplay
{
    /// <summary>
    ///     Formats the type of an error, typically supplied as the
    ///     <see cref="Error.Type" /> value, in a short and human-
    ///     readable form.
    /// </summary>
    /// <remarks>
    ///     Typically, exception type names can be long to display and
    ///     complex to consume. The essential part can usually be found in
    ///     the start of an exception type name minus its namespace. For
    ///     example, a human reading the string,
    ///     "System.Runtime.InteropServices.COMException", will usually
    ///     considers "COM" as the most useful component of the entire
    ///     type name. This method does exactly that. It assumes that the
    ///     the input type is a .NET Framework exception type name where
    ///     the namespace and class will be separated by the last
    ///     period (.) and where the type name ends in "Exception". If
    ///     these conditions are method then a string like,
    ///     "System.Web.HttpException" will be transformed into simply
    ///     "Html".
    /// </remarks>
    public static string HumaneExceptionErrorType(string type)
    {
        if (type == null || type.Length == 0)
        {
            return string.Empty;
        }

        var lastDotIndex = CultureInfo.InvariantCulture.CompareInfo.LastIndexOf(type, '.');

        if (lastDotIndex > 0)
        {
            type = type.Substring(lastDotIndex + 1);
        }

        const string conventionalSuffix = "Exception";

        if (type.Length > conventionalSuffix.Length)
        {
            var suffixIndex = type.Length - conventionalSuffix.Length;

            if (string.Compare(type, suffixIndex, conventionalSuffix, 0,
                conventionalSuffix.Length, true, CultureInfo.InvariantCulture) == 0)
            {
                type = type.Substring(0, suffixIndex);
            }
        }

        return type;
    }

    /// <summary>
    ///     Formats the error type of an <see cref="Error" /> object in a
    ///     short and human-readable form.
    /// </summary>
    public static string HumaneExceptionErrorType(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return HumaneExceptionErrorType(error.Type);
    }
}