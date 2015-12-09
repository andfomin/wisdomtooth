using System;
using MediaCurator.Data.SQLite;
using MediaCurator.Common;

namespace MediaCurator.Processor
{
    public class BrowserHelper
    {
        public static void SaveMessage(Guid documentId, Guid messageType, string message)
        {
            var document = new BrowserHelperMessageDocument
            {
                MessageType = messageType,
                DocumentId = documentId,
                Message = message,
            };

            using (var connection = new Connection(CommonSettings.DatabaseFilePath))
            {
                connection.Documents.Write(document);
            }
        }
    }
}
