
namespace MediaCurator.Data
{
    // Compatible with the wire types of Google Protocol Buffers.
    enum WireType
    {
        Varint = 0,
        Fixed64 = 1,
        LengthDelimited = 2,
        Fixed32 = 5,
    }
}
