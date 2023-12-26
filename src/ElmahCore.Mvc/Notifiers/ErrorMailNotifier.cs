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

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading;

namespace ElmahCore.Mvc.Notifiers;

/// <summary>
///     HTTP module that sends an e-mail whenever an unhandled exception
///     occurs in an ASP.NET web application.
/// </summary>
public class ErrorMailNotifier : IErrorNotifier
{
    private readonly bool _reportAsynchronously;

    /// <summary>
    ///     Initializes the module and prepares it to handle requests.
    /// </summary>
    public ErrorMailNotifier(string name, EmailOptions options)
    {
        Name = name;
        //
        // Extract the settings.
        //

        MailRecipient = options.MailRecipient;
        MailSender = options.MailSender;
        MailCopyRecipient = options.MailCopyRecipient;
        MailSubjectFormat = options.MailSubjectFormat;
        MailPriority = options.MailPriority;
        _reportAsynchronously = options.ReportAsynchronously;
        SmtpServer = options.SmtpServer;
        SmtpPort = options.SmtpPort;
        AuthUserName = options.AuthUserName;
        AuthPassword = options.AuthPassword;
        UseSsl = options.UseSsl;
    }

    /// <summary>
    ///     Gets the e-mail address of the sender.
    /// </summary>
    protected virtual string? MailSender { get; }

    /// <summary>
    ///     Gets the e-mail address of the recipient, or a
    ///     comma-/semicolon-delimited list of e-mail addresses in case of
    ///     multiple recipients.
    /// </summary>
    /// <remarks>
    ///     When using System.Web.Mail components under .NET Framework 1.x,
    ///     multiple recipients must be semicolon-delimited.
    ///     When using System.Net.Mail components under .NET Framework 2.0
    ///     or later, multiple recipients must be comma-delimited.
    /// </remarks>
    protected virtual string? MailRecipient { get; }

    /// <summary>
    ///     Gets the e-mail address of the recipient for mail carbon
    ///     copy (CC), or a comma-/semicolon-delimited list of e-mail
    ///     addresses in case of multiple recipients.
    /// </summary>
    /// <remarks>
    ///     When using System.Web.Mail components under .NET Framework 1.x,
    ///     multiple recipients must be semicolon-delimited.
    ///     When using System.Net.Mail components under .NET Framework 2.0
    ///     or later, multiple recipients must be comma-delimited.
    /// </remarks>
    protected virtual string? MailCopyRecipient { get; }

    /// <summary>
    ///     Gets the text used to format the e-mail subject.
    /// </summary>
    /// <remarks>
    ///     The subject text specification may include {0} where the
    ///     error message (<see cref="Error.Message" />) should be inserted
    ///     and {1} <see cref="Error.Type" /> where the error type should
    ///     be insert.
    /// </remarks>
    protected virtual string? MailSubjectFormat { get; }

    /// <summary>
    ///     Gets the priority of the e-mail.
    /// </summary>
    protected virtual MailPriority MailPriority { get; }

    /// <summary>
    ///     Gets the SMTP server host name used when sending the mail.
    /// </summary>
    protected string? SmtpServer { get; }

    /// <summary>
    ///     Gets the SMTP port used when sending the mail.
    /// </summary>
    protected int SmtpPort { get; }

    /// <summary>
    ///     Gets the user name to use if the SMTP server requires authentication.
    /// </summary>
    protected string? AuthUserName { get; }

    /// <summary>
    ///     Gets the clear-text password to use if the SMTP server requires
    ///     authentication.
    /// </summary>
    protected string? AuthPassword { get; }

    /// <summary>
    ///     Determines if SSL will be used to encrypt communication with the
    ///     mail server.
    /// </summary>
    protected bool UseSsl { get; }

    public void Notify(Error error)
    {
        if (_reportAsynchronously)
        {
            ReportErrorAsync(error);
        }
        else
        {
            ReportError(error);
        }
    }

    public string Name { get; }

    /// <summary>
    ///     Schedules the error to be e-mailed asynchronously.
    /// </summary>
    /// <remarks>
    ///     The default implementation uses the <see cref="ThreadPool" />
    ///     to queue the reporting.
    /// </remarks>
    protected virtual void ReportErrorAsync(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        //
        // Schedule the reporting at a later time using a worker from 
        // the system thread pool. This makes the implementation
        // simpler, but it might have an impact on reducing the
        // number of workers available for processing ASP.NET
        // requests in the case where lots of errors being generated.
        //

        ThreadPool.QueueUserWorkItem(ReportError, error);
    }

    private void ReportError(object? state)
    {
        try
        {
            ReportError((Error) state!);
        }

        //
        // Catch and trace COM/SmtpException here because this
        // method will be called on a thread pool thread and
        // can either fail silently in 1.x or with a big band in
        // 2.0. For latter, see the following MS KB article for
        // details:
        //
        //     Unhandled exceptions cause ASP.NET-based applications 
        //     to unexpectedly quit in the .NET Framework 2.0
        //     http://support.microsoft.com/kb/911816
        //

        catch (SmtpException e)
        {
            Trace.TraceError(e.ToString());
        }
    }

    /// <summary>
    ///     Schedules the error to be e-mailed synchronously.
    /// </summary>
    protected virtual void ReportError(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        //
        // Start by checking if we have a sender and a recipient.
        // These values may be null if someone overrides the
        // implementation of OnInit but does not override the
        // MailSender and MailRecipient properties.
        //

        var sender = MailSender ?? string.Empty;
        var recipient = MailRecipient ?? string.Empty;
        var copyRecipient = MailCopyRecipient ?? string.Empty;

        if (recipient.Length == 0)
        {
            return;
        }

        //
        // Create the mail, setting up the sender and recipient and priority.
        //

        var mail = new MailMessage();
        mail.Priority = MailPriority;

        mail.From = new MailAddress(sender);
        var recipients = recipient.Split(';');
        foreach (var r in recipients)
        {
            mail.To.Add(r);
        }

        if (copyRecipient.Length > 0)
        {
            recipients = copyRecipient.Split(';');
            foreach (var r in recipients)
            {
                mail.CC.Add(r);
            }
        }

        //
        // Format the mail subject.
        // 

        var subjectFormat = MailSubjectFormat ?? "Error ({1}): {0}";
        mail.Subject = string.Format(subjectFormat, error.Message, error.Type).Replace('\r', ' ')
            .Replace('\n', ' ');

        //
        // Format the mail body.
        //

        var formatter = CreateErrorFormatter();

        var bodyWriter = new StringWriter();
        formatter.Format(bodyWriter, error);
        mail.Body = bodyWriter.ToString();

        switch (formatter.MimeType)
        {
            case MediaTypeNames.Text.Html:
                mail.IsBodyHtml = true;
                break;
            case MediaTypeNames.Text.Plain:
                mail.IsBodyHtml = false;
                break;

            default:
            {
                throw new ApplicationException(string.Format(
                    "The error mail module does not know how to handle the {1} media type that is created by the {0} formatter.",
                    formatter.GetType().FullName, formatter.MimeType));
            }
        }

        try
        {
            //
            // Send off the mail with some chance to pre- or post-process
            // using event.
            //
            SendMail(mail);
        }
        finally
        {
            mail.Dispose();
        }
    }

    /// <summary>
    ///     Creates the <see cref="ErrorTextFormatter" /> implementation to
    ///     be used to format the body of the e-mail.
    /// </summary>
    internal virtual ErrorTextFormatter CreateErrorFormatter()
    {
        return new ErrorMailHtmlFormatter();
    }

    /// <summary>
    ///     Sends the e-mail using SmtpMail or SmtpClient.
    /// </summary>
    protected virtual void SendMail(MailMessage mail)
    {
        ArgumentNullException.ThrowIfNull(mail);

        //
        // Under .NET Framework 2.0, the authentication settings
        // go on the SmtpClient object rather than mail message
        // so these have to be set up here.
        //

        var client = new SmtpClient();

        var host = SmtpServer ?? string.Empty;

        if (host.Length > 0)
        {
            client.Host = host;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
        }

        var port = SmtpPort;
        if (port > 0)
        {
            client.Port = port;
        }

        var userName = AuthUserName ?? string.Empty;
        var password = AuthPassword ?? string.Empty;

        if (userName.Length > 0 && password.Length > 0)
        {
            client.Credentials = new NetworkCredential(userName, password);
        }

        client.EnableSsl = UseSsl;

        client.Send(mail);
    }
}

public class EmailOptions
{
    public string? MailRecipient { get; set; }
    public string? MailSender { get; set; }
    public string? MailCopyRecipient { get; set; }
    public string? MailSubjectFormat { get; set; }
    public MailPriority MailPriority { get; set; }
    public bool ReportAsynchronously { get; set; }
    public string? SmtpServer { get; set; }
    public int SmtpPort { get; set; }
    public string? AuthUserName { get; set; }
    public string? AuthPassword { get; set; }
    public bool UseSsl { get; set; }
}