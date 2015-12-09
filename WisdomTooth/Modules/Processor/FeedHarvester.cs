using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using MediaCurator.Common;
using MediaCurator.Data.SQLite;

namespace MediaCurator.Processor
{
    public class FeedHarvester
    {

        public void Process()
        {
            using (var connection = new Connection(CommonSettings.DatabaseFilePath))
            {
                HarvestFeeds(connection);
            }
        }

        private void HarvestFeeds(Connection connection)
        {
            ////var harvestDoc = connection.Documents.Select<FeedHarvest>().SingleOrDefault();

            ////// On the first run.
            ////if (harvestDoc == null)
            ////{
            ////    harvestDoc = new DocumentRecord<FeedHarvest>();
            ////    connection.Documents.Insert(harvestDoc);
            ////    harvestDoc = connection.Documents.Select<FeedHarvest>().SingleOrDefault();
            ////};

            ////var newRun = DateTime.UtcNow.AddSeconds(-10);
            ////var newEvents = connection.Events.Select(harvestDoc.Document.LastRun, newRun, EventTypes.HelperScriptResults, null);
            ////var harvestedFeeds = ExtractFeedsFromEvents(newEvents);

            ////var newFeeds = harvestedFeeds
            ////    .Where(i => connection.Documents.Count<FeedSubscription>(i, null) == 0)
            ////    .Select(i => new DocumentRecord<FeedSubscription> 
            ////    {
            ////        Subject = i,
            ////    }).ToList();

            ////connection.BeginTransaction();
            ////try
            ////{
            ////    foreach (var item in newFeeds)
            ////    {
            ////        connection.Documents.Insert(item);
            ////    }

            ////    harvestDoc.Document.LastRun = newRun;
            ////    connection.Documents.Update(harvestDoc);

            ////    connection.CommitTransactionNoFlush();
            ////}
            ////catch (Exception ex)
            ////{
            ////    connection.RollbackTransaction();
            ////    throw new MediaCuratorException(1702815557, ex);
            ////}
        }

        private IEnumerable<string> ExtractFeedsFromEvents(IEnumerable<object> events)
        {
            List<string> feeds = new List<string>();
            ////foreach (var item in events)
            ////{
            ////    var root = XElement.Parse(item.Data);
            ////    if (root != null && root.Name == "message")
            ////    {
            ////        foreach (var feedRoot in root.Elements("feeds"))
            ////        {
            ////            feeds.AddRange(feedRoot.Elements("feed").Select(i => i.Value.ToLower()));
            ////        }
            ////    }
            ////}
            return feeds;
        }

    }
}
