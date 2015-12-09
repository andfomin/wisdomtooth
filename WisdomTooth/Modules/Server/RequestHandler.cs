using System;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
using System.Diagnostics;
using MediaCurator.Common;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MediaCurator.Server
{
    class RequestHandler
    {
        // The order does matter, the first matching pattern will be choosen.
        // TODO: Remove "/mediacurator" and reverse the order, so the first entry will match most of times.
        public static readonly Dictionary<string, Action<HttpListenerContext, CancellationToken>> Handlers =
            new Dictionary<string, Action<HttpListenerContext, CancellationToken>>
            {
                    { "/mediacurator/helper", BrowserHelper.BrowserHelperMessageHandler.HandleRequest },
                    { "/mediacurator/update", Update.HandleRequest },
                    { "/mediacurator", HomePage.HandleRequest },
            };

        public static void ProcessRequest(HttpListenerContext context, CancellationToken cancelToken)
        {
            HttpListenerResponse response = context.Response;
            string rawUrl = context.Request.RawUrl;

            try
            {
                foreach (var i in Handlers)
                {
                    if (rawUrl.StartsWith(i.Key))
                    {
                        i.Value(context, cancelToken);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                // Do not rethrow. Produce response anyway. Client will inspect StatusCode.
                if (response.StatusCode == (int)HttpStatusCode.OK)
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }
                Trace.TraceError(MediaCuratorException.ExceptionMessage(ex));
            }
            finally
            {
                // HttpListenerResponse holds unmanaged resources. Always call Close() or Abort() to prevent a memory leak.
                response.Close();
            }

        }

    }
}
