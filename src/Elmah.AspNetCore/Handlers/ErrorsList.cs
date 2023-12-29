using System.Collections.Generic;

namespace Elmah.AspNetCore.Handlers;

internal class ErrorsList
{
    public List<ErrorLogEntryWrapper> Errors { get; set; } = default!;
    public int TotalCount { get; set; }
}