using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
        public XmlFileErrorLog(IOptions<XmlFileErrorLogOptions> options, IWebHostEnvironment hostingEnvironment)
        {
            _logPath = options.Value.LogPath;
            if (_logPath.StartsWith("~/"))
            {
                _logPath = Path.Combine(hostingEnvironment.WebRootPath ?? hostingEnvironment.ContentRootPath,
                    _logPath.Substring(2));
            }
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
        ///     sortable date and a unique identifier. Currently the XML files are stored indefinitely.
        ///     As they are stored as files, they may be managed using standard scheduled jobs.
        /// </remarks>
        public override Task LogAsync(Guid id, Error error, CancellationToken cancellationToken)
        {
            var logPath = LogPath;
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }

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

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Returns a page of errors from the folder in descending order
        ///     of logged time as defined by the sortable file names.
        /// </summary>
        public override Task<int> GetErrorsAsync(string? searchText, List<ErrorLogFilter> filters, int errorIndex, int pageSize,
            ICollection<ErrorLogEntry> errorEntryList, CancellationToken cancellationToken)
        {
            if (errorIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(errorIndex), errorIndex, null);
            }

            if (pageSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, null);
            }

            var logPath = LogPath;
            var dir = new DirectoryInfo(logPath);
            if (!dir.Exists)
            {
                return Task.FromResult(0);
            }

            var infos = dir.GetFiles("error-*.xml");
            if (!infos.Any())
            {
                return Task.FromResult(0);
            }

            var files = infos.Where(info => IsUserFile(info.Attributes))
                .OrderBy(info => info.Name, StringComparer.OrdinalIgnoreCase)
                .Select(info => Path.Combine(logPath, info.Name))
                .Reverse()
                .ToArray();

            if (errorEntryList == null)
            {
                return Task.FromResult(files.Length); // Return total
            }

            int totalCount;
            IEnumerable<ErrorLogEntry> entries;
            if (filters.Count == 0 && string.IsNullOrEmpty(searchText))
            {
                entries = files.Skip(errorIndex)
                    .Take(pageSize)
                    .Select(LoadErrorLogEntry);
                totalCount = files.Length;
            }
            else
            {
                var fEntries = files
                    .Select(LoadErrorLogEntry)
                    .Where(e => e is not null && ErrorLogFilterHelper.IsMatched(e, searchText, filters))
                    .ToList();
                totalCount = fEntries.Count;
                
                entries = fEntries
                    .Skip(errorIndex)
                    .Take(pageSize);
            }

            foreach (var entry in entries)
            {
                errorEntryList.Add(entry);
            }

            return Task.FromResult(totalCount); // Return total
        }

        private ErrorLogEntry? LoadErrorLogEntry(string path)
        {
            for (var i = 0; i < 5; i++)
            {
                try
                {
                    using var reader = XmlReader.Create(path, new XmlReaderSettings() { CheckCharacters = false });
                    if (!reader.IsStartElement("error"))
                    {
                        return null;
                    }

                    var id = reader.GetAttribute("errorId");
                    var error = ErrorXml.Decode(reader);
                    return new ErrorLogEntry(this, id, error);
                }
                catch (IOException)
                {
                    //ignored
                    Task.Delay(500).GetAwaiter().GetResult();
                }
            }

            throw new IOException("");
        }

        /// <summary>
        ///     Returns the specified error from the filesystem, or throws an exception if it does not exist.
        /// </summary>
        public override Task<ErrorLogEntry?> GetErrorAsync(string id, CancellationToken cancellationToken)
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
            {
                return Task.FromResult<ErrorLogEntry?>(null);
            }

            if (!IsUserFile(file.Attributes))
            {
                return Task.FromResult<ErrorLogEntry?>(null);
            }

            using var reader = XmlReader.Create(file.FullName, new XmlReaderSettings() { CheckCharacters = false });
            return Task.FromResult<ErrorLogEntry?>(new ErrorLogEntry(this, id, ErrorXml.Decode(reader)));
        }

        private static bool IsUserFile(FileAttributes attributes)
        {
            return 0 == (attributes & (FileAttributes.Directory |
                                       FileAttributes.Hidden |
                                       FileAttributes.System));
        }
    }
}