using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaCurator.Data;
using MediaCurator.Common;

namespace MediaCurator.Processor
{
    class FeedSubscription : MediaCurator.Data.IDocument
    {
        private static readonly Guid DocumentType = new Guid("{E08E6CDE-C7BB-4ADF-B01E-349E18129554}");

        private int? id;

        public int? Id
        {
            get { return id; }
        }

        public Guid Type
        {
            get { return DocumentType; }
        }

        public void SetIdOnce(int value)
        {
            if (id.HasValue)
            {
                throw new MediaCuratorException("5185529");
            }
            id = value;
        }

        // 1
        public DateTime LastDownloadAttempt { get; set; }

        public void Serialize(ProtobufEncoder encoder)
        {
            encoder.Write(1, LastDownloadAttempt);
        }

        public void Deserialize(ProtobufDecoder decoder)
        {
            decoder.Read(new Dictionary<uint, Action<byte[]>>
            {
                {1, (b) => { LastDownloadAttempt = DateTime.FromBinary(BitConverter.ToInt64(b, 0)); } },
            });
        }


        public Lucene.Net.Documents.Document ToIndex()
        {
            throw new NotImplementedException();
        }
    }
}
