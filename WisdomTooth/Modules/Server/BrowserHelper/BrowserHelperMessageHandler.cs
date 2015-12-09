using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using MediaCurator.Common;
using MediaCurator.Processor;

namespace MediaCurator.Server.BrowserHelper
{
    internal class BrowserHelperMessageHandler
    {
        // Custom request header to distinguish each page. Carries a GUID.
        private const string documenIdCustomHeader = "X-MediaCurator-DocumentId";

        public static readonly Dictionary<Guid, Func<Guid, Guid, string, CancellationToken, string>> MessageHandlers =
            new Dictionary<Guid, Func<Guid, Guid, string, CancellationToken, string>>
                {
                    { EventTypes.HelperOnLoad, OnLoadHandler.HandleRequest },
                    { EventTypes.HelperScriptResults, CustomScriptResultsHandler.HandleRequest },
                };

        public static void HandleRequest(HttpListenerContext context, CancellationToken cancelToken)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            Encoding encoding = Encoding.UTF8;
            response.ContentEncoding = encoding;

            var headerValue = request.Headers[documenIdCustomHeader];
            Guid documentId;
            bool isGuid = Guid.TryParse(headerValue, out documentId);
            // Check two conditions: a documentId for the page and the body is expected to have data.
            if (!isGuid || !request.HasEntityBody)
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                throw new MediaCuratorException("28183431");
            }

            // Read request as text.
            string requestText;
            using (Stream requestStream = request.InputStream)
            {
                using (StreamReader reader = new StreamReader(requestStream, encoding))
                {
                    requestText = reader.ReadToEnd();
                }
            }

            cancelToken.ThrowIfCancellationRequested();

            // Handle the message by a handler corresponding to its type.
            Guid messageType = BrowserHelperMessageHandler.GetMessageType(requestText);

            cancelToken.ThrowIfCancellationRequested();

            string responseText = string.Empty;
            Func<Guid, Guid, string, CancellationToken, string> messageHandler;
            if (BrowserHelperMessageHandler.MessageHandlers.TryGetValue(messageType, out messageHandler))
            {
                responseText = messageHandler.Invoke(documentId, messageType, requestText, cancelToken);
            }
            else
            {
                throw new MediaCuratorException("28174859");
            }

            var buffer = encoding.GetBytes(responseText);
            if (response.OutputStream.CanWrite)
            {
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
            }
        }

        public static Guid GetMessageType(string message)
        {
            Guid result = Guid.Empty;
            using (XmlTextReader reader = new XmlTextReader(new StringReader(message)))
            {
                // read only the root node
                if (reader.MoveToContent() == XmlNodeType.Element && reader.Name == "message")
                {
                    if (reader.MoveToAttribute("type"))
                    {
                        result = XmlConvert.ToGuid(reader.Value);
                    }
                }
            }
            return result;
        }

        protected static void SaveMessageData(Guid documentId, Guid messageType, string message, CancellationToken cancelToken)
        {
            cancelToken.ThrowIfCancellationRequested();
            MediaCurator.Processor.BrowserHelper.SaveMessage(documentId, messageType, message);
        }
    }
}
