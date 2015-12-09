namespace MediaCurator.Data
{
    using System;

    public interface IDocument
    {
        int? Id { get; }
        Guid Type { get; }
        void SetIdOnce(int value);
        void Serialize(ProtobufEncoder encoder);
        void Deserialize(ProtobufDecoder decoder);
        Lucene.Net.Documents.Document ToIndex();
    }

    /*
    Implementation sample:
      
    private static readonly Guid DocumentType = new Guid("{XXXXXXX}");

    private int? id;

    public int? Id
    {
        get { return this.id; }
    }

    public Guid Type
    {
        get { return DocumentType; }
    }      

    public void SetIdOnce(int value)
    {
        if (this.id.HasValue)
        {
            throw new MediaCuratorException("00000000");
        }
        this.id = value;
    }

    public void Serialize(ProtobufEncoder encoder)
    {
        encoder.Write(1, Id);
        encoder.Write(2, Name);            
        foreach (var item in Items)
        {
            encoder.Write(3, item);
        }
        encoder.WriteEmbedded(1, ChildObject);
    }
 
    public void Unserialize(ProtobufDecoder decoder)
    {
        decoder.Read(new Dictionary<uint, Action<byte[]>>
        {
            {1, (buffer) => { SomeId = BitConverter.ToInt32(buffer, 0); } },
            {2, (buffer) => { SomeGuid = new Guid(buffer); } },     
        });
                    SomeString = decoder.DecodeString(buffer);
                    SomeDateTime = DateTime.FromBinary(BitConverter.ToInt64(buffer, 0));
                    Items = new List<int>().Add(BitConverter.ToInt32(buffer, 0));
                    ChildObject = Document<Class1>FromBytes(buffer);
    }
    */
}
