namespace MediaCurator.Data
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Collections.Generic;

    /// <summary>
    /// Reads stream created by Encoder. Reads values by one at a time. Reading is orchestrated by the caller.
    /// The wire protocol is similar to Google Protocol Buffers.
    /// </summary>
    public class ProtobufDecoder
    {
        private readonly Stream stream;
        private readonly UTF8Encoding encoding;

        public ProtobufDecoder(Stream stream)
        {
            Debug.Assert(stream != null, "13351849");
            this.stream = stream;
            this.encoding = new UTF8Encoding(false, true);
        }

        public void Read(Dictionary<uint, Action<byte[]>> actions)
        {
            uint tag;
            byte[] bytes;
            while (this.TryReadValue(out tag, out bytes))
            {
                if (actions.ContainsKey(tag))
                {
                    actions[tag].Invoke(bytes);
                }
            }
        }

        /// <summary>
        /// Converts strings from UTF-8 to Unicode.
        /// </summary>
        /// <param name="bytes">
        /// The buffer obtained from a call to TryReadValue().
        /// </param>
        /// <returns>
        /// Unicode string.
        /// </returns>
        public string DecodeString(byte[] bytes)
        {
            return this.encoding.GetString(bytes);
        }

        /// <summary>
        /// Reads one property value at a time.
        /// </summary>
        /// <param name="tag">
        /// Returns the custom tag of the property.
        /// </param>
        /// <param name="buffer">
        /// Returns the buffer containing the property data. The exact data type is unknown and interpetated by the caller.
        /// </param>
        /// <returns>True is success. False if an error occured. If false, the caller must stop reading further to avoid unpredictable results.
        /// </returns>
        private bool TryReadValue(out uint tag, out byte[] buffer)
        {
            tag = uint.MaxValue;
            buffer = null;

            uint key;
            bool result = this.TryReadVarint(out key);
            if (result)
            {
                tag = key >> 3;

                int keyWireType = (int)key & 0x07;
                result = Enum.IsDefined(typeof(WireType), keyWireType);
                if (result)
                {
                    WireType wireType = (WireType)keyWireType;
                    switch (wireType)
                    {
                        case WireType.Varint:
                            uint varint;
                            result = this.TryReadVarint(out varint);
                            if (result)
                            {
                                // Reverse ZigZag encoding.
                                int value = (int)(varint >> 1) ^ -(int)(varint & 1L);
                                buffer = BitConverter.GetBytes(value);
                            }

                            break;

                        case WireType.Fixed64:
                        case WireType.Fixed32:
                            result = this.TryReadFixed(wireType, out buffer);
                            break;

                        case WireType.LengthDelimited:
                            result = this.TryReadLengthDelimited(out buffer);
                            break;

                        default:
                            result = false;
                            break;
                    }
                }
            }

            return result;
        }

        private bool TryReadVarint(out uint value)
        {
            value = 0;
            int shift = 0;
            int chunk;
            do
            {
                chunk = this.stream.ReadByte();

                // -1 if at the end of the stream.
                if (chunk < 0 || shift > 32)
                {
                    return false;
                }

                value |= (uint)(chunk & 0x7F) << shift;
                shift += 7;
            }
            while ((chunk & 0x80) != 0);

            return true;
        }

        private bool TryReadFixed(WireType wireType, out byte[] buffer)
        {
            int size = wireType == WireType.Fixed64 ? 8 : (wireType == WireType.Fixed32 ? 4 : 0);
            Debug.Assert(size != 0, "11349643");
            buffer = new byte[size];
            int read = this.stream.Read(buffer, 0, size);
            return read == size;
        }

        /// <summary>
        /// Reads bytes from the stream.
        /// </summary>
        /// <param name="buffer">
        /// Out parameter. Returns binary buffer. If it is a string, it is represented as UTF-8 encoded bytes.
        /// </param>
        /// <returns>True if success</returns>
        private bool TryReadLengthDelimited(out byte[] buffer)
        {
            uint length;
            bool result = this.TryReadVarint(out length);
            buffer = result ? new byte[length] : null;
            if (result)
            {
                int read = this.stream.Read(buffer, 0, (int)length);
                result = read == length;
            }

            return result;
        }
    }
}
