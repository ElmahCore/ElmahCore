using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace ElmahCore
{
    /// <summary>
    ///     An <see cref="ErrorLog" /> implementation that uses XML files stored on
    ///     disk as its backing store.
    /// </summary>

    // ReSharper disable once UnusedType.Global
    public class XmlFileErrorLog : ErrorLog
    {
        private readonly string _logPath;

        /// <summary>
        ///     Initializes a new instance of the <see cref="XmlFileErrorLog" /> class
        ///     using a dictionary of configured settings.
        /// </summary>
        public XmlFileErrorLog(IOptions<ElmahOptions> options, IHostingEnvironment hostingEnvironment)
        {
            _logPath = options.Value.LogPath;
            if (_logPath.StartsWith("~/"))
                _logPath = Path.Combine(hostingEnvironment.WebRootPath ?? hostingEnvironment.ContentRootPath,
                    _logPath.Substring(2));
        }


        /// <summary>
        ///     Gets the path to where the log is stored.
        /// </summary>

        protected virtual string LogPath => _logPath;

        /// <summary>
        ///     Gets the name of this error log implementation.
        /// </summary>

        public override string Name => "XML File-Based Error Log";

        /// <summary>
        ///     Logs an error to the database.
        /// </summary>
        /// <remarks>
        ///     Logs an error as a single XML file stored in a folder. XML files are named with a
        ///     sortable date and a unique identifier. Currently the XML files are stored indefinately.
        ///     As they are stored as files, they may be managed using standard scheduled jobs.
        /// </remarks>
        public override string Log(Error error)
        {
            var errorId = Guid.NewGuid();

            Log(errorId, error);

            return errorId.ToString();
        }

        public override void Log(Guid id, Error error)
        {
            var logPath = LogPath;
            if (!Directory.Exists(logPath))
                Directory.CreateDirectory(logPath);

            var timeStamp = error.Time > DateTime.MinValue ? error.Time : DateTime.Now;

            var fileName = string.Format(CultureInfo.InvariantCulture,
                @"error-{0:yyyy-MM-ddHHmmssZ}-{1}.xml",
                /* 0 */ timeStamp.ToUniversalTime(),
                /* 1 */ id);

            var path = Path.Combine(logPath, fileName);

            try
            {
                using var writer = new XmlTextWriter(path, Encoding.UTF8) {Formatting = Formatting.Indented};
                writer.WriteStartElement("error");
                writer.WriteAttributeString("errorId", id.ToString());
                ErrorXml.Encode(error, writer);
                writer.WriteEndElement();
                writer.Flush();
            }
            catch (IOException)
            {
                // If an IOException is thrown during writing the file,
                // it means that we will have an either empty or
                // partially written XML file on disk. In both cases,
                // the file won't be valid and would cause an error in
                // the UI.
                File.Delete(path);
                throw;
            }
        }

        /// <summary>
        ///     Returns a page of errors from the folder in descending order
        ///     of logged time as defined by the sortable file names.
        /// </summary>
        public override int GetErrors(int errorIndex, int pageSize, ICollection<ErrorLogEntry> errorEntryList)
        {
            if (errorIndex < 0) throw new ArgumentOutOfRangeException(nameof(errorIndex), errorIndex, null);
            if (pageSize < 0) throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, null);

            var logPath = LogPath;
            var dir = new DirectoryInfo(logPath);
            if (!dir.Exists)
                return 0;

            var infos = dir.GetFiles("error-*.xml");
            if (!infos.Any())
                return 0;

            var files = infos.Where(info => IsUserFile(info.Attributes))
                .OrderBy(info => info.Name, StringComparer.OrdinalIgnoreCase)
                .Select(info => Path.Combine(logPath, info.Name))
                .Reverse()
                .ToArray();

            if (errorEntryList == null) return files.Length; // Return total

            var entries = files.Skip(errorIndex)
                .Take(pageSize)
                .Select(LoadErrorLogEntry);

            foreach (var entry in entries)
                errorEntryList.Add(entry);

            return files.Length; // Return total
        }

        private ErrorLogEntry LoadErrorLogEntry(string path)
        {
            for (var i = 0; i < 5; i++)
                try
                {
                    using var reader = XmlReader.Create(path, new XmlReaderSettings() { CheckCharacters = false });
                    if (!reader.IsStartElement("error"))
                        return null;

                    var id = reader.GetAttribute("errorId");
                    var error = ErrorXml.Decode(reader);
                    return new ErrorLogEntry(this, id, error);
                }
                catch (IOException)
                {
                    //ignored
                    Task.Delay(500).GetAwaiter().GetResult();
                }

            throw new IOException("");
        }

        /// <summary>
        ///     Returns the specified error from the filesystem, or throws an exception if it does not exist.
        /// </summary>
        public override ErrorLogEntry GetError(string id)
        {
            try
            {
                id = new Guid(id).ToString(); // validate GUID
            }
            catch (FormatException e)
            {
                throw new ArgumentException(e.Message, id, e);
            }

            var file = new DirectoryInfo(LogPath).GetFiles($"error-*-{id}.xml")
                .FirstOrDefault();

            if (file == null)
                return null;

            if (!IsUserFile(file.Attributes))
                return null;

            using var reader = XmlReader.Create(file.FullName, new XmlReaderSettings() { CheckCharacters = false });
            return new ErrorLogEntry(this, id, ErrorXml.Decode(reader));
        }

        private static bool IsUserFile(FileAttributes attributes)
        {
            return 0 == (attributes & (FileAttributes.Directory |
                                       FileAttributes.Hidden |
                                       FileAttributes.System));
        }
    }
}