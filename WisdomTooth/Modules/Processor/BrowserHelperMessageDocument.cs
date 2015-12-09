using System;
using System.Collections.Generic;
using MediaCurator.Data;
using MediaCurator.Common;

namespace MediaCurator.Processor
{
    public class BrowserHelperMessageDocument : MediaCurator.Data.IDocument
    {
        public const string DocumentTypeString = "A031F3AC-5A70-4E3E-87A6-21E8F0335A4B";
        public static readonly Guid DocumentType = new Guid(DocumentTypeString);

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
        public Guid MessageType { get; set; }

        // 2
        public Guid DocumentId { get; set; }

        // 3
        public string Message { get; set; }

        public void SetIdOnce(int value)
        {
            if (id.HasValue)
            {
                throw new MediaCuratorException("05220022");
            }
            id = value;
        }

        public void Serialize(ProtobufEncoder encoder)
        {
            encoder.Write(1, MessageType);
            encoder.Write(2, DocumentId);
            encoder.Write(3, Message);
        }

        public void Deserialize(ProtobufDecoder decoder)
        {
            decoder.Read(new Dictionary<uint, Action<byte[]>>
            {
                {1, (buffer) => { MessageType = new Guid(buffer); } },
                {2, (buffer) => { DocumentId = new Guid(buffer); } },
                {3, (buffer) => { Message = decoder.DecodeString(buffer); } },
            });
        }


        public Lucene.Net.Documents.Document ToIndex()
        {
            throw new NotImplementedException();
        }
    }
}
