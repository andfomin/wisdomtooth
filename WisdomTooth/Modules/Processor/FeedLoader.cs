using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaCurator.Data.SQLite;
using MediaCurator.Common;

namespace MediaCurator.Processor
{
    class FeedLoader
    {
        public void Process()
        {
            using (var connection = new Connection(CommonSettings.DatabaseFilePath))
            {
                LoadFeeds(connection);
            }
        }

        private void LoadFeeds(Connection connection)
        {
            var q = connection.Documents.Read<FeedSubscription>(0);
        }

        /*        public void DoIt()
                {
                    ////Stream stream = new StreamReader(File1).BaseStream;
                    Stream stream = Load(UriSlashdot);
                    ////Console.WriteLine(new StreamReader(stream).ReadToEnd());
                    XmlReader reader = new RobustXmlReader(stream);
                    reader.MoveToContent();

                    Type feedType = typeof(ExtendedSyndicationFeed);
                    SyndicationFeed feed;

                    if (!TryRead(reader, new Rss20FeedFormatter(feedType), out feed))
                    {
                        if (!TryRead(reader, new Atom10FeedFormatter(feedType), out feed))
                        {
                            if (!TryRead(reader, new Rss10FeedFormatter(feedType), out feed))
                            {
                            }
                        }
                    }

                    if (feed != null)
                    {
                        foreach (var item in feed.Items)
                        {
                            Console.WriteLine(item.PublishDate.ToString());
                        }
                    }
                }

                private static bool TryRead(XmlReader reader, SyndicationFeedFormatter formatter, out SyndicationFeed feed)
                {
                    bool result = formatter.CanRead(reader);
                    feed = null;
                    if (result)
                    {
                        formatter.ReadFrom(reader);
                        feed = formatter.Feed;
                    }
                    return result;
                }
        */


    }
}
