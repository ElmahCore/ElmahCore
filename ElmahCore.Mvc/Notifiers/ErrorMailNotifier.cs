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
using System.Net.Mail;

namespace ElmahCore.Mvc.Notifiers
{
    #region Imports

	using MailAttachment = Attachment;
	using ThreadPool = System.Threading.ThreadPool;
	using Encoding = System.Text.Encoding;
    using NetworkCredential = System.Net.NetworkCredential;

    #endregion

    /// <summary>
    /// HTTP module that sends an e-mail whenever an unhandled exception
    /// occurs in an ASP.NET web application.
    /// </summary>

    public class ErrorMailNotifier : IErrorNotifier
    {
        private string _mailSender;
        private string _mailRecipient;
        private string _mailCopyRecipient;
        private string _mailSubjectFormat;
        private MailPriority _mailPriority;
        private bool _reportAsynchronously;
        private string _smtpServer;
        private int _smtpPort;
        private string _authUserName;
        private string _authPassword;
        private bool _noYsod;
        private bool _useSsl;

        /// <summary>
        /// Initializes the module and prepares it to handle requests.
        /// </summary>
        
        public ErrorMailNotifier(string name, EmailOptions options)
        {
            Name = name;
            //
            // Extract the settings.
            //
         

            _mailRecipient = options.MailRecipient;
            _mailSender = options.MailSender;
            _mailCopyRecipient = options.MailCopyRecipient;
            _mailSubjectFormat = options.MailSubjectFormat;
            _mailPriority = options.MailPriority;
            _reportAsynchronously = options.ReportAsynchronously;
            _smtpServer = options.SmtpServer;
            _smtpPort = options.SmtpPort;
            _authUserName = options.AuthUserName;
            _authPassword = options.AuthPassword;
            _noYsod = options.SendYsod;
            _useSsl = options.UseSsl;
        }
        


        /// <summary>
        /// Gets the e-mail address of the sender.
        /// </summary>
        
        protected virtual string MailSender
        {
            get { return _mailSender; }
        }

        /// <summary>
        /// Gets the e-mail address of the recipient, or a 
        /// comma-/semicolon-delimited list of e-mail addresses in case of 
        /// multiple recipients.
        /// </summary>
        /// <remarks>
        /// When using System.Web.Mail components under .NET Framework 1.x, 
        /// multiple recipients must be semicolon-delimited.
        /// When using System.Net.Mail components under .NET Framework 2.0
        /// or later, multiple recipients must be comma-delimited.
        /// </remarks>

        protected virtual string MailRecipient
        {
            get { return _mailRecipient; }
        }

        /// <summary>
        /// Gets the e-mail address of the recipient for mail carbon 
        /// copy (CC), or a comma-/semicolon-delimited list of e-mail 
        /// addresses in case of multiple recipients.
        /// </summary>
        /// <remarks>
        /// When using System.Web.Mail components under .NET Framework 1.x, 
        /// multiple recipients must be semicolon-delimited.
        /// When using System.Net.Mail components under .NET Framework 2.0
        /// or later, multiple recipients must be comma-delimited.
        /// </remarks>

        protected virtual string MailCopyRecipient
        {
            get { return _mailCopyRecipient; }
        }

        /// <summary>
        /// Gets the text used to format the e-mail subject.
        /// </summary>
        /// <remarks>
        /// The subject text specification may include {0} where the
        /// error message (<see cref="Error.Message"/>) should be inserted 
        /// and {1} <see cref="Error.Type"/> where the error type should 
        /// be insert.
        /// </remarks>

        protected virtual string MailSubjectFormat
        {
            get { return _mailSubjectFormat; }
        }

        /// <summary>
        /// Gets the priority of the e-mail. 
        /// </summary>
        
        protected virtual MailPriority MailPriority
        {
            get { return _mailPriority; }
        }

        /// <summary>
        /// Gets the SMTP server host name used when sending the mail.
        /// </summary>

        protected string SmtpServer
        {
            get { return _smtpServer; }
        }

        /// <summary>
        /// Gets the SMTP port used when sending the mail.
        /// </summary>

        protected int SmtpPort
        {
            get { return _smtpPort; }
        }

        /// <summary>
        /// Gets the user name to use if the SMTP server requires authentication.
        /// </summary>

        protected string AuthUserName
        {
            get { return _authUserName; }
        }

        /// <summary>
        /// Gets the clear-text password to use if the SMTP server requires 
        /// authentication.
        /// </summary>

        protected string AuthPassword
        {
            get { return _authPassword; }
        }

        /// <summary>
        /// Indicates whether <a href="http://en.wikipedia.org/wiki/Screens_of_death#ASP.NET">YSOD</a> 
        /// is attached to the e-mail or not. If <c>true</c>, the YSOD is 
        /// not attached.
        /// </summary>
        
        protected bool NoYsod
        {
            get { return _noYsod; }
        }

        /// <summary>
        /// Determines if SSL will be used to encrypt communication with the 
        /// mail server.
        /// </summary>

        protected bool UseSsl
        {
            get { return _useSsl; }
        }




        /// <summary>
        /// Schedules the error to be e-mailed asynchronously.
        /// </summary>
        /// <remarks>
        /// The default implementation uses the <see cref="ThreadPool"/>
        /// to queue the reporting.
        /// </remarks>

        protected virtual void ReportErrorAsync(Error error)
        {
            if (error == null)
                throw new ArgumentNullException("error");

            //
            // Schedule the reporting at a later time using a worker from 
            // the system thread pool. This makes the implementation
            // simpler, but it might have an impact on reducing the
            // number of workers available for processing ASP.NET
            // requests in the case where lots of errors being generated.
            //

            ThreadPool.QueueUserWorkItem(ReportError, error);
        }

        private void ReportError(object state)
        {
            try
            {
                ReportError((Error) state);
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
        /// Schedules the error to be e-mailed synchronously.
        /// </summary>

        protected virtual void ReportError(Error error)
        {
            if (error == null)
                throw new ArgumentNullException("error");

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
                return;

            //
            // Create the mail, setting up the sender and recipient and priority.
            //

            var mail = new MailMessage();
            mail.Priority = MailPriority;

            mail.From = new MailAddress(sender);
            mail.To.Add(recipient);
            
            if (copyRecipient.Length > 0)
                mail.CC.Add(copyRecipient);

            //
            // Format the mail subject.
            // 

            var subjectFormat = MailSubjectFormat ?? "Error ({1}): {0}";
            mail.Subject = string.Format(subjectFormat, error.Message, error.Type).
                Replace('\r', ' ').Replace('\n', ' ');

            //
            // Format the mail body.
            //

            var formatter = CreateErrorFormatter();

            var bodyWriter = new StringWriter();
            formatter.Format(bodyWriter, error);
            mail.Body = bodyWriter.ToString();

            switch (formatter.MimeType)
            {
                case "text/html": mail.IsBodyHtml = true; break;
                case "text/plain": mail.IsBodyHtml = false; break;

                default :
                {
                    throw new ApplicationException(string.Format(
                        "The error mail module does not know how to handle the {1} media type that is created by the {0} formatter.",
                        formatter.GetType().FullName, formatter.MimeType));
                }
            }


            try
            {
                //
                // If an HTML message was supplied by the web host then attach 
                // it to the mail if not explicitly told not to do so.
                //

                if (!NoYsod && error.WebHostHtmlMessage.Length > 0)
                {
                    var ysodAttachment = CreateHtmlAttachment("YSOD", error.WebHostHtmlMessage);

                    if (ysodAttachment != null)
                        mail.Attachments.Add(ysodAttachment);
                }

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

        private static MailAttachment CreateHtmlAttachment(string name, string html)
        {

            return MailAttachment.CreateAttachmentFromString(html,
                name + ".html", Encoding.UTF8, "text/html");
        }

        /// <summary>
        /// Creates the <see cref="ErrorTextFormatter"/> implementation to 
        /// be used to format the body of the e-mail.
        /// </summary>

        internal virtual ErrorTextFormatter CreateErrorFormatter()
        {
            return new ErrorMailHtmlFormatter();
        }

        /// <summary>
        /// Sends the e-mail using SmtpMail or SmtpClient.
        /// </summary>

        protected virtual void SendMail(MailMessage mail)
        {
            if (mail == null)
                throw new ArgumentNullException("mail");

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
                client.Port = port;

            var userName = AuthUserName ?? string.Empty;
            var password = AuthPassword ?? string.Empty;

            if (userName.Length > 0 && password.Length > 0)
                client.Credentials = new NetworkCredential(userName, password);

            client.EnableSsl = UseSsl;

            client.Send(mail);
        }


        public void Notify(Error error)
        {
            if (_reportAsynchronously)
                ReportErrorAsync(error);
            else
                ReportError(error);
        }

        public string Name { get; }
    }

    public class EmailOptions
    {
        public string MailRecipient { get; set; }
        public string MailSender { get; set; }
        public string MailCopyRecipient { get; set; }
        public string MailSubjectFormat { get; set; }
        public MailPriority MailPriority { get; set; }
        public bool ReportAsynchronously { get; set; }
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string AuthUserName { get; set; }
        public string AuthPassword { get; set; }
        public bool SendYsod { get; set; }
        public bool UseSsl { get; set; }
    }
}
