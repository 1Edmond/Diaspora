using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Constants;

public static class CachedKeyConstants
{
    public static string UpdloadDocumentIdForAwaitingTaskCompletedForKey = "procedure:document-output";
    public static TimeSpan UpdloadDocumentIdForAwaitingTaskCompletedForKeyDuration = TimeSpan.FromMinutes(2);
}
