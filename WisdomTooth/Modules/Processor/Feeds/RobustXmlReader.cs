using System;
using System.Linq;

namespace MediaCurator.Processor.Feeds
{
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.ServiceModel.Syndication;
    using System.Xml;

    public class RobustXmlReader : System.Xml.XmlTextReader
    {
        private readonly string[] rss20DateTimeHints = { "pubDate", "lastBuildDate" };
        private readonly string[] atom10DateTimeHints = { "updated", "published", "atom:updated" };
        private readonly string[] localeTags = { "language" };
        private const string SyndicationTimeFormat = "R";

        private readonly MethodInfo rss20Method;
        private readonly MethodInfo atom10Method;

        private bool resolveRss20DateTime;
        private bool resolveAtom10DateTime;
        private bool resolveLocale;
        private CultureInfo cultureInfo = CultureInfo.CurrentCulture;

        private Atom10FeedFormatter atom10FeedFormatterInstance;

        public RobustXmlReader(Stream input)
            : base(input)
        {
            this.rss20Method = typeof(Rss20FeedFormatter).GetMethod("DateFromString", BindingFlags.NonPublic | BindingFlags.Static);
            Debug.Assert(this.rss20Method != null);
            this.atom10Method = typeof(Atom10FeedFormatter).GetMethod("DateFromString", BindingFlags.NonPublic | BindingFlags.Instance);
            Debug.Assert(this.atom10Method != null);
        }

        private Atom10FeedFormatter Atom10FeedFormatterInstance
        {
            get
            {
                return this.atom10FeedFormatterInstance
                    ?? (this.atom10FeedFormatterInstance = new Atom10FeedFormatter());
            }
        }

        public override bool IsStartElement(string localname, string ns)
        {
            this.resolveRss20DateTime = false;
            this.resolveAtom10DateTime = false;
            this.resolveLocale = false;

            bool result = base.IsStartElement(localname, ns);
            if (result)
            {
                if (string.Equals(ns, string.Empty)
                    && this.rss20DateTimeHints.Any(i => string.Equals(i, localname, StringComparison.InvariantCultureIgnoreCase)))
                {
                    this.resolveRss20DateTime = true;
                }
                else if (this.atom10DateTimeHints.Any(i => string.Equals(i, localname, StringComparison.InvariantCultureIgnoreCase)))
                {
                    this.resolveAtom10DateTime = true;
                }
                else if (this.localeTags.Any(i => string.Equals(i, localname, StringComparison.InvariantCultureIgnoreCase)))
                {
                    this.resolveLocale = true;
                }
            }

            return result;
        }

        public override string ReadString()
        {
            string value = base.ReadString();

            if (this.resolveLocale)
            {
                try
                {
                    this.cultureInfo = new CultureInfo(value);
                }
                catch (CultureNotFoundException)
                {
                }
            }

            if (this.resolveRss20DateTime && this.resolveAtom10DateTime)
            {
                try
                {
                    // ReSharper disable AssignNullToNotNullAttribute
                    if (this.resolveRss20DateTime)
                    {
                        this.rss20Method.Invoke(null, 0, null, new object[] { value, this }, this.cultureInfo);
                    }

                    if (this.resolveAtom10DateTime)
                    {
                        this.atom10Method.Invoke(
                            this.Atom10FeedFormatterInstance, 0, null, new object[] { value, this }, this.cultureInfo);
                    }

                    // ReSharper restore AssignNullToNotNullAttribute
                }
                catch (TargetInvocationException)
                {
                    string[] formats = 
                    {
                        "M/d/yyyy hh:mm:ss GMT",  /* 1/1/2010 00:00:00 GMT */
                        "ddd, dd MMM yyyy HH:mm:ss GMT 00:00:00 GMT",  /* Mon, 30 Apr 2010 22:40:50 GMT 00:00:00 GMT */
                    };

                    DateTimeOffset dateTimeOffset;
                    bool parsed = DateTimeOffset.TryParseExact(value, formats, this.cultureInfo, DateTimeStyles.None, out dateTimeOffset);

                    if (!parsed)
                    {
                        Debug.Write(value);
                    }

                    value = parsed
                        ? dateTimeOffset.ToString(SyndicationTimeFormat)
                        /* We can't fail reading in the Release version. Better we return an incorrect date */
                        : DateTimeOffset.UtcNow.ToString(SyndicationTimeFormat);
                }
            }

            return value;
        }
    }
}