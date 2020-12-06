using System;
using System.Text;

namespace PSW.BinarySave
{
    // Terminology : 
    // Decoder = Decodes byte array to a class
    // Encoder = Encodes class a to byte array

    // Generic delegates make it very extensible for any developer to define
    // how their app treat bytes into data.
    public delegate T BinaryDecoder<out T>(in byte[] source);
    public delegate byte[] BinaryEncoder<T>(in T source);

    // Contain decoder and encoder for basic data types like int, long, string (uses BitConverter)
    public static class BasicCodec
    {
        // Short / Int16
        public static BinaryDecoder<short> ShortDec = (in byte[] source) => BitConverter.ToInt16(source, 0);
        public static BinaryEncoder<short> ShortEnc = (in short source) => BitConverter.GetBytes(source);
        
        // Short / Int16
        public static BinaryDecoder<byte> ByteDec = (in byte[] source) => source[0];
        public static BinaryEncoder<byte> ByteEnc = (in byte source) => new byte[] { source };

        // Integer / Int32
        public static BinaryDecoder<int> IntDec = (in byte[] source) => BitConverter.ToInt32(source, 0);
        public static BinaryEncoder<int> IntEnc = (in int source) => BitConverter.GetBytes(source);
        
        // Long / Int64
        public static BinaryDecoder<long> LongDec = (in byte[] source) => BitConverter.ToInt64(source, 0);
        public static BinaryEncoder<long> LongEnc = (in long source) => BitConverter.GetBytes(source);

        // UShort / UInt16
        public static BinaryDecoder<ushort> UShortDec = (in byte[] source) => BitConverter.ToUInt16(source, 0);
        public static BinaryEncoder<ushort> UShortEnc = (in ushort source) => BitConverter.GetBytes(source);

        // UInteger / UInt32
        public static BinaryDecoder<uint> UIntDec = (in byte[] source) => BitConverter.ToUInt32(source, 0);
        public static BinaryEncoder<uint> UIntEnc = (in uint source) => BitConverter.GetBytes(source);

        // ULong / UInt64
        public static BinaryDecoder<ulong> ULongDec = (in byte[] source) => BitConverter.ToUInt64(source, 0);
        public static BinaryEncoder<ulong> ULongEnc = (in ulong source) => BitConverter.GetBytes(source);

        // Float / Single
        public static BinaryDecoder<float> FloatDec = (in byte[] source) => BitConverter.ToSingle(source, 0);
        public static BinaryEncoder<float> FloatEnc = (in float source) => BitConverter.GetBytes(source);

        // Double
        public static BinaryDecoder<double> DoubleDec = (in byte[] source) => BitConverter.ToDouble(source, 0);
        public static BinaryEncoder<double> DoubleEnc = (in double source) => BitConverter.GetBytes(source);

        // Boolean
        public static BinaryDecoder<bool> BoolDec = (in byte[] source) => source[0] != 0x00;
        public static BinaryEncoder<bool> BoolEnc = (in bool source) => new byte[] { (byte)(source ? 0xFF : 0x00) };
        
    }

    public static class StringCodec
    {
        // Unicode Little Ending String
        public static BinaryDecoder<string> UCSLEDec = (in byte[] source) => Encoding.Unicode.GetString(source);
        public static BinaryEncoder<string> UCSLEEnc = (in string source) => Encoding.Unicode.GetBytes(source);

        // Unicode Little Ending String
        public static BinaryDecoder<string> UCSBEDec = (in byte[] source) => Encoding.BigEndianUnicode.GetString(source);
        public static BinaryEncoder<string> UCSBEEnc = (in string source) => Encoding.BigEndianUnicode.GetBytes(source);
        
        // ASCII String
        public static BinaryDecoder<string> ASCIIDec = (in byte[] source) => Encoding.ASCII.GetString(source);
        public static BinaryEncoder<string> ASCIIEnc = (in string source) => Encoding.ASCII.GetBytes(source);

        // UTF8 String
        public static BinaryDecoder<string> UTF8Dec = (in byte[] source) => Encoding.UTF8.GetString(source);
        public static BinaryEncoder<string> UTF8Enc = (in string source) => Encoding.UTF8.GetBytes(source);

        // UTF7 String
        public static BinaryDecoder<string> UTF7Dec = (in byte[] source) => Encoding.UTF7.GetString(source);
        public static BinaryEncoder<string> UTF7Enc = (in string source) => Encoding.UTF7.GetBytes(source);

        // UTF32 String
        public static BinaryDecoder<string> UTF32Dec = (in byte[] source) => Encoding.UTF32.GetString(source);
        public static BinaryEncoder<string> UTF32Enc = (in string source) => Encoding.UTF32.GetBytes(source);
    }
}
