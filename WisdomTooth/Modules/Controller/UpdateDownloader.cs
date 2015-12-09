using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using MediaCurator.Common;
using System.IO;
using System.Xml.Linq;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using Bits3_0.Interop;
using System.Threading;
using System.Runtime.InteropServices;

namespace MediaCurator.Controller
{
    public class UpdateDownloader
    {
        private const string BaseManifestUrl = "http://www.fomin-family.com/update/manifest.xml";

        public string GetDownloadUrl()
        {
            ////// Upload some values.
            ////NameValueCollection form = new NameValueCollection();
            ////form.Add("MyName", "MyValue");
            ////WebClient client = new WebClient();
            ////Byte[] responseData = client.UploadValues("+http://www.contoso.com/form.aspx", form);

            string result = "";
            try
            {
                string antiCacheUrl = string.Format("{0}?t={1}", BaseManifestUrl, DateTime.UtcNow.ToString("yyMMddHHmmss"));
                var doc = XDocument.Load(antiCacheUrl);

                var root = doc.Element("Update");

                if (root != null)
                {
                    Assembly assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly(); 
                    Version current = assembly.GetName().Version;

                    foreach (var i in root.Elements())
                    {
                        bool passed = true;

                        var min = i.Element("Min");
                        passed = passed && (min != null) && IsPassed(current, min, ((c, v) => c > v));

                        var max = i.Element("Max");
                        passed = passed && (max != null) && IsPassed(current, max, ((c, v) => c < v));

                        var urlAttr = i.Attribute("Url");
                        if (passed && (urlAttr != null))
                        {
                            result = urlAttr.Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(MediaCuratorException.ExceptionMessage(new MediaCuratorException(0331123666, ex)));
            }

            return result;
        }

        private bool IsPassed(Version current, XElement element, Func<Version, Version, bool> func)
        {
            bool result = false;

            var ver = element.Attribute("Ver");
            var include = element.Attribute("Include");

            if (ver != null || include != null)
            {
                bool includeVersion = false;
                includeVersion = XmlConvert.ToBoolean(include.Value);

                Version version;
                if (Version.TryParse(ver.Value, out version))
                {
                    result = func(current, version) || (includeVersion ? current == version : false);
                }
            }
            return result;
        }

        public void IinitiateDownload(string url, string filePath)
        {
            var manager = new BackgroundCopyManager3_0();
            try
            {
                Bits3_0.Interop.GUID jobId;
                IBackgroundCopyJob initialJob;
                manager.CreateJob("MediaCuratorUpdate", BG_JOB_TYPE.BG_JOB_TYPE_DOWNLOAD, out jobId, out initialJob);
                
                // Imitate suspended workflow.
                Marshal.FinalReleaseComObject(initialJob);
                IBackgroundCopyJob job;
                manager.GetJob(jobId, out job);

                try
                {
                    job.AddFile(url, filePath);
                    job.Resume();

                    BG_JOB_STATE state;
                    do
                    {
                        job.GetState(out state);
                        Thread.Sleep(2000);

                    } while (state != BG_JOB_STATE.BG_JOB_STATE_TRANSFERRED);

                    job.Complete();

                }
                catch (Exception)
                {
                    job.Cancel();
                }
                finally
                {
                    Marshal.FinalReleaseComObject(job);
                }
            }
            finally
            {
                Marshal.FinalReleaseComObject(manager);
            }
        }

    }




}
