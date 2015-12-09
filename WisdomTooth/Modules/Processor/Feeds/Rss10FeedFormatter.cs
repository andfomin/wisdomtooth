namespace MediaCurator.Processor.Feeds
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.ServiceModel.Syndication;
    using System.Xml;

    public class Rss10FeedFormatter : System.ServiceModel.Syndication.SyndicationFeedFormatter
    {
        private const string RssVersion = "Rss10";
        private const string RdfNamespaceUri = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
        private const string NamespaceUri = "http://purl.org/rss/1.0/";

        private readonly Type feedType;

        public Rss10FeedFormatter()
        {
        }

        public Rss10FeedFormatter(Type feedTypeToCreate)
        {
            this.feedType = feedTypeToCreate;
        }

        public override string Version
        {
            get
            {
                return RssVersion;
            }
        }

        public override bool CanRead(XmlReader reader)
        {
            Debug.Assert(reader != null, "11503549");
            return reader.IsStartElement("RDF", RdfNamespaceUri);
        }

        protected override SyndicationFeed CreateFeedInstance()
        {
            return (SyndicationFeed)Activator.CreateInstance(this.feedType ?? typeof(SyndicationFeed));
        }

        public override void ReadFrom(XmlReader reader)
        {
            Debug.Assert(reader != null, "4155047");

            reader.MoveToContent();

            if (!this.CanRead(reader))
            {
                throw new XmlException("4120549");
            }

            this.SetFeed(this.CreateFeedInstance());

            /* Read in <RDF> */
            reader.ReadStartElement();
            reader.ReadStartElement("channel");

            /* Process <channel> children */
            while (reader.IsStartElement())
            {
                if (reader.IsStartElement("title"))
                {
                    Feed.Title = new TextSyndicationContent(reader.ReadElementString());
                }
                else if (reader.IsStartElement("link"))
                {
                    Feed.Links.Add(new SyndicationLink(new Uri(reader.ReadElementString())));
                }
                else if (reader.IsStartElement("description"))
                {
                    Feed.Description = new TextSyndicationContent(reader.ReadElementString());
                }
                else
                {
                    reader.Skip();
                }
            }

            /* Read in </channel> */
            reader.ReadEndElement();

            List<SyndicationItem> items = new List<SyndicationItem>();
            Feed.Items = items;

            while (reader.IsStartElement())
            {
                while (reader.IsStartElement("item"))
                {
                    SyndicationItem item = CreateItem(Feed);
                    items.Add(item);

                    reader.ReadStartElement();
                    while (reader.IsStartElement())
                    {
                        if (reader.IsStartElement("title"))
                        {
                            item.Title = new TextSyndicationContent(reader.ReadElementString());
                        }
                        else if (reader.IsStartElement("link"))
                        {
                            item.Links.Add(new SyndicationLink(new Uri(reader.ReadElementString())));
                        }
                        else if (reader.IsStartElement("description"))
                        {
                            item.Summary = new TextSyndicationContent(reader.ReadElementString());
                        }
                        else
                        {
                            reader.Skip();
                        }
                    }

                    reader.ReadEndElement();
                }

                reader.Skip();
            }
        }

        public override void WriteTo(System.Xml.XmlWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
