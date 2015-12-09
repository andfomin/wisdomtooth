using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.IO;

namespace MediaCurator.Server
{
    public class HomePage
    {
        private const string HomePageFile =
 @"C:\Users\Andrey\Documents\MediaCurator\Client\Main\WisdomTooth\Modules\Server\HomePage\homepage.html";

        public static void HandleRequest(HttpListenerContext context, CancellationToken cancelToken)
        {
            HttpListenerResponse response = context.Response;

            Encoding encoding = Encoding.UTF8;
            response.ContentEncoding = encoding;

            var buffer = File.ReadAllBytes(HomePageFile);

            if (response.OutputStream.CanWrite)
            {
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
            }
        }


    }
}
