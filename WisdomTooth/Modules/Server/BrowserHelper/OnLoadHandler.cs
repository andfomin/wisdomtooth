using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaCurator.Server.BrowserHelper
{
    internal class OnLoadHandler : MediaCurator.Server.BrowserHelper.BrowserHelperMessageHandler
    {
        internal static string HandleRequest(Guid documentId, Guid messageType, string message, CancellationToken cancelToken)
        {
            // Finish HTTP response ASAP, delegate processing to another thread.
            Task.Factory.StartNew(() => SaveMessageData(documentId, messageType, message, cancelToken));

            StringBuilder responseBuilder = new StringBuilder();
            responseBuilder.AppendFormat("{0}{1}{2}{3}{4}",
                Scripts.ScriptBegin,
                Scripts.XmlElemScript,
                Scripts.OnLoadScript,
                Scripts.ScriptToExecute,
                Scripts.ScriptEnd);
            return responseBuilder.ToString();
        }
    }
}
