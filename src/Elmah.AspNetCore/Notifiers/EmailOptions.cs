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

//[assembly: Elmah.Scc("$Id: ErrorMailModule.cs 923 2011-12-23 22:02:10Z azizatif $")]

using System.Net.Mail;

namespace Elmah.AspNetCore.Notifiers;

public class EmailOptions
{
    public string? MailRecipient { get; set; }
    public string? MailSender { get; set; }
    public string? MailCopyRecipient { get; set; }
    public string? MailSubjectFormat { get; set; }
    public MailPriority MailPriority { get; set; }
    public string? SmtpServer { get; set; }
    public int SmtpPort { get; set; }
    public string? AuthUserName { get; set; }
    public string? AuthPassword { get; set; }
    public bool UseSsl { get; set; }
}