namespace Elmah.Memory;

public class MemoryErrorLogOptions
{
    /// <summary>
    /// Gets or sets the number of error entries to keep in memory.
    /// </summary>
    public int Size { get; set; } = 15;
}