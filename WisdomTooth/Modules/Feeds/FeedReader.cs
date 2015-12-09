using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Syndication;
using System.Xml;
//using Microsoft.Feeds.Interop;
using System.Diagnostics;

namespace MediaCurator.Feeds
{
  public  class FeedReader
    {
        public void DoIt()
        {
            XmlReader reader = XmlReader.Create("http://feeds.dzone.com/dzone/frontpage");
            SyndicationFeed feed = SyndicationFeed.Load(reader);

            foreach (SyndicationItem item in feed.Items)
            {
                Debug.Print(item.Title.Text);
            }

           // Rss20FeedFormatter
        }

    }
}
