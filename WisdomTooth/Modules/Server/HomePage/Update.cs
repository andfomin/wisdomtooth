using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using MediaCurator.Common;

namespace MediaCurator.Server
{
    public class Update
    {

        private const string ResponseText = @"
<!DOCTYPE HTML PUBLIC &quot;-//W3C//DTD HTML 4.0 Transitional//EN&quot;>
<html>
	<head>
		<title></title>
	</head>
	<body>
Update
	</body>
</html>
";

        public static void HandleRequest(HttpListenerContext context, CancellationToken cancelToken)
        {
            HttpListenerResponse response = context.Response;

            Encoding encoding = Encoding.UTF8;
            response.ContentEncoding = encoding;

            var buffer = encoding.GetBytes(ResponseText);

            if (response.OutputStream.CanWrite)
            {
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
            }

            Task.Factory.StartNew(() => StartInstaller());
        }

        private static void StartInstaller()
        {
            try
            {
                // Simple "singleton" :)
                IEnumerable<string> files = Directory.EnumerateFiles(CommonSettings.RootDataDir, "*.msi", SearchOption.AllDirectories);
                if (files.Count() == 1)
                {
                    Process myProcess = new Process();
                    /* When you use the operating system shell to start processes, you can start any document (which is any registered 
                     * file type associated with an executable that has a default open action) and perform operations on the file, 
                     * such as printing, by using the Process object. When UseShellExecute is false, you can start only executables by 
                     * using the Process object. */
                    myProcess.StartInfo.UseShellExecute = true;
                    myProcess.StartInfo.FileName = files.Single();
                    myProcess.Start();
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(MediaCuratorException.ExceptionMessage(22226676, ex));
            }
        }

    }
}
