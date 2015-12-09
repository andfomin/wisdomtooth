using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MediaCurator.Server.BrowserHelper
{
    internal class CustomScriptResultsHandler : MediaCurator.Server.BrowserHelper.BrowserHelperMessageHandler
    {
        internal static string HandleRequest(Guid documentId, Guid messageType, string message, CancellationToken cancelToken)
        {
            // Finish HTTP response ASAP, delegate processing to another thread.
            Task.Factory.StartNew(() => SaveMessageData(documentId, messageType, message, cancelToken));

            return string.Empty;
        }
    }
}
