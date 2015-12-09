namespace MediaCurator.Data
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Encodes values in a way similar to the encoding for Google Protocol Buffers. Default values are bypassed and not written.
    /// </summary>
    public class ProtobufEncoder
    {
        /* Although System.IO.BinaryWriter seems to be a good candidate for this purpose, it has a few disadvantages. 
         * The BinaryWriter will take ownership of any stream that you pass to it: when disposed, it will also dispose the stream.
         * Its Write7BitEncodedInt is protected. It is intended for writing Int32 string length and does not use ZigZag encoding for negative numbers.
         * There is no support in BinaryWriter for injecting tag+wiretype markers.
        */

        private readonly Stream stream;
        private readonly UTF8Encoding encoding;

        public ProtobufEncoder(Stream stream)
        {
            Debug.Assert(stream != null, "09242056");
            this.stream = stream;
            this.encoding = new UTF8Encoding(false, true); 
        }

        public void Write(uint tag, byte[] value)
        {
            if (value != null)
            {
                int length = value.Length;
                if (length != 0)
                {
                    this.WriteKey(tag, WireType.LengthDelimited);
                    this.WriteVarint((uint)length);
                    this.stream.Write(value, 0, length);
                }
            }
        }

        public void Write(uint tag, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                this.Write(tag, this.encoding.GetBytes(value));
            }
        }

        public void Write(uint tag, int value)
        {
            if (value != default(int))
            {
                // ZigZag encoding maps signed integers to unsigned integers so that numbers with a small absolute value have a small varint encoded value too.
                uint zz = (uint)((value << 1) ^ (value >> 31));
                this.WriteKey(tag, WireType.Varint);
                this.WriteVarint(zz);
            }
        }

        public void Write(uint tag, float value)
        {
            if (value != default(float))
            {
                this.WriteFixed(tag, BitConverter.GetBytes(value), WireType.Fixed32);
            }
        }

        public void Write(uint tag, double value)
        {
            if (value != default(double))
            {
                this.WriteFixed(tag, BitConverter.GetBytes(value), WireType.Fixed64);
            }
        }

        public void WriteFixed(uint tag, int value)
        {
            if (value != default(int))
            {
                this.WriteFixed(tag, BitConverter.GetBytes(value), WireType.Fixed32);
            }
        }

        public void WriteFixed(uint tag, long value)
        {
            if (value != default(long))
            {
                this.WriteFixed(tag, BitConverter.GetBytes(value), WireType.Fixed64);
            }
        }

        public void Write(uint tag, bool value)
        {
            if (value != default(bool))
            {
                this.Write(tag, 1);
            }
        }

        public void Write(uint tag, Guid value)
        {
            if (value != default(Guid) /*Guid.Empty*/)
            {
                this.Write(tag, value.ToByteArray());
            }
        }

        /// <summary>
        /// Notice: If a local DateTime object is serialized in one time zone (by the ToBinary method), and then deserialized in a different time zone (by the FromBinary method), the local time represented by the resulting DateTime object is automatically adjusted to the second time zone.
        /// </summary>
        /// <param name="tag">
        /// The property tag.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        public void Write(uint tag, DateTime value)
        {
            if (value != default(DateTime))
            {
                this.WriteFixed(tag, value.ToBinary());
            }
        }

        public void Write(uint tag, TimeSpan value)
        {
            if (value != default(TimeSpan))
            {
                this.WriteFixed(tag, value.Ticks);
            }
        }

        public void Write(uint tag, IDocument value)
        {
            if (value != null)
            {
                using (var tempStream = new MemoryStream())
                {
                    value.Serialize(new ProtobufEncoder(tempStream));
                    /* The initial size of the stream's buffer may be more than the actual size of the data written. */
                    long length = tempStream.Position;
                    if (length != 0)
                    {
                        this.WriteKey(tag, WireType.LengthDelimited);
                        this.WriteVarint((uint)length);
                        this.stream.Write(tempStream.GetBuffer(), 0, (int)length);
                    }
                }
            }
        }

        private void WriteKey(uint tag, WireType wireType)
        {
            this.WriteVarint((tag << 3) | (uint)wireType);
        }

        private void WriteVarint(uint value)
        {
            byte[] buffer = new byte[5];
            int count = 0;
            while (true)
            {
                buffer[count] = (byte)(value & 0x7F);
                value >>= 7;
                if (value == 0)
                {
                    break;
                }

                buffer[count++] |= 0x80;  // More bytes to come.
            }

            this.stream.Write(buffer, 0, count + 1);
        }

        private void WriteFixed(uint tag, byte[] value, WireType wireType)
        {
            int size = wireType == WireType.Fixed64 ? 8 : (wireType == WireType.Fixed32 ? 4 : 0);
            if ((value != null) && (size != 0))
            {
                this.WriteKey(tag, wireType);
                this.stream.Write(value, 0, size);
            }
        }
    }
}
