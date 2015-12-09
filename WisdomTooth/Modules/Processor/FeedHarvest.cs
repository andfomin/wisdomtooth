using System;
using System.Text;
using MediaCurator.Data;
using System.Collections.Generic;
using MediaCurator.Common;

namespace MediaCurator.Processor
{
    public class FeedHarvest : MediaCurator.Data.IDocument
    {
        private static readonly Guid DocumentType = new Guid("{2AC88EF9-1F95-427F-B392-AF06EA55296F}");

        private int? id;

        public int? Id
        {
            get { return id; }
        }

        public Guid Type
        {
            get { return DocumentType; }
        }

        // 1
        public DateTime LastRun { get; set; }

        public void SetIdOnce(int value)
        {
            if (id.HasValue)
            {
                throw new MediaCuratorException("5185331");
            }
            id = value;
        }

        public void Serialize(ProtobufEncoder encoder)
        {
            encoder.Write(1, LastRun);
        }

        public void Deserialize(ProtobufDecoder decoder)
        {
            decoder.Read(new Dictionary<uint, Action<byte[]>>
            {
                {1, (b) => { LastRun = DateTime.FromBinary(BitConverter.ToInt64(b, 0)); } },
            });
        }

        public Lucene.Net.Documents.Document ToIndex()
        {
            throw new NotImplementedException();
        }
    }
}
