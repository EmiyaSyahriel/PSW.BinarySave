using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PSW.BinarySave
{
    /**
     * Simple Run-Length Content-Tabled Format (Default formatting)
     * 
     * Format : 
     * union {
     *      int32 dataSize;
     *      int32 keyLength;
     *      ascii keyString;
     * } tocData;
     * 
     * int32 dataCount;
     * tocData[dataCount] keys;
     * int8[dataCount][dataSize] values;
     */
    public static class RLCTFormat
    {
        private static readonly byte[] buffer4 = new byte[4];
        private static byte[] buffer = new byte[32];

        public static readonly BinaryDecoder<Dictionary<string, byte[]>> Decoder =
        (in byte[] source) =>
        {
            Dictionary<string, byte[]> retval = new Dictionary<string, byte[]>();
            List<int> sizes = new List<int>();
            List<string> keys = new List<string>();
            using (MemoryStream str = new MemoryStream(source))
            {
                int read = str.Read(buffer4, 0, 4);
                if(read >= 4)
                {
                    int dataCount = BitConverter.ToInt32(buffer4, 0);
                    for(int i =0; i < dataCount; i++)
                    {
                        read = str.Read(buffer4, 0, 4);
                        if(read >= 4)
                        {
                            int byteSize = BitConverter.ToInt32(buffer4, 0);
                            read = str.Read(buffer4, 0, 4);
                            if (read >= 4)
                            {
                                int keyLength = BitConverter.ToInt32(buffer4, 0);
                                buffer = new byte[keyLength];
                                read = str.Read(buffer, 0, keyLength);
                                if (read >= keyLength)
                                {
                                    string key = Encoding.ASCII.GetString(buffer);
                                    sizes.Add(byteSize);
                                    keys.Add(key);
                                }
                            }
                        }
                    }
                    for(int i = 0; i < dataCount; i++)
                    {
                        int size = sizes[i];
                        buffer = new byte[size];
                        read = str.Read(buffer, 0, size);
                        if (read >= size) {
                            retval.Add(keys[i], buffer);
                        }
                    }
                }
            }            
            return retval;
        };
        
        public static readonly BinaryEncoder<Dictionary<string, byte[]>> Encoder =
        (in Dictionary<string, byte[]> source) =>
        {
            byte[] retval = new byte[0];
            using(MemoryStream str = new MemoryStream())
            {
                var keys = source.Keys.ToArray();
                var values = source.Values.ToArray();
                str.Write(BitConverter.GetBytes(keys.Length), 0, 4);
                for(int i = 0; i < keys.Length; i++)
                {
                    var key = keys[i];
                    var value = values[i];
                    byte[] keyBytes = Encoding.ASCII.GetBytes(keys[i]);
                    str.Write(BitConverter.GetBytes(value.Length), 0, 4);
                    str.Write(BitConverter.GetBytes(keyBytes.Length), 0, 4);
                    str.Write(keyBytes, 0, keyBytes.Length);
                }
                for(int i = 0; i < keys.Length; i++)
                {
                    var value = values[i];
                    str.Write(value, 0, value.Length);
                }
                retval = str.ToArray();
            }
            return retval;
        };
    }
}
