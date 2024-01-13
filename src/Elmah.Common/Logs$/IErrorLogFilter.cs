namespace Elmah;

public interface IErrorLogFilter
{
    /// <summary>
    /// Evaluates all filter predicates against the <paramref name="entry"/> and returns <c>true</c> if the entry passes.
    /// </summary>
    /// <param name="entry">The error log entry</param>
    /// <returns><c>true</c> if the error log entry matches all predicates; <c>false</c> otherwise</returns>
    bool IsMatch(ErrorLogEntry entry);
}
