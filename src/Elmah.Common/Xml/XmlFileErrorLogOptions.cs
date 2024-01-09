namespace Elmah.Xml;

public class XmlFileErrorLogOptions
{
    /// <summary>
    ///     ELMAH log files path (default = '"~/errors.xml"'), example: options.LogPath = "~/log"; // OR options.LogPath =
    ///     "с:\errors";
    /// </summary>
    public string LogPath { get; set; } = "~/errors.xml";
}
