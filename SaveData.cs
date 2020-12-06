using System.IO;
using System.Collections.Generic;
using System;

namespace PSW.BinarySave
{
    using Patches;
    /*****
     * Generic Dictionary-based binary save data
     * 
     * Uses a generic delegates that convert data from (or) bytes. 
     * the downside is that every key is in the same dictionary which make developers 
     * should make a very unique key for every of data they will save.
     * 
     * Possibly caused data misread since byte is an abstract data which can be freely interpreted
     **/
    public sealed class SaveData
    {
        /** Cached instance of the class */
        private static SaveData _instance;
        private static byte[] header = { 80, 83, 87, 0 };
        /** Getter for the cached instance, creates new if not cached yet. */
        private static SaveData instance
        {
            get
            {
                if (_instance == null) _instance = new SaveData();
                return _instance;
            }
        }

        public static BinaryDecoder<Dictionary<string, byte[]>> fileBytesDecoder = RLCTFormat.Decoder;
        public static BinaryEncoder<Dictionary<string, byte[]>> fileBytesEncoder = RLCTFormat.Encoder;
        public static string SaveDataPath = @"D:\";
        public static string SaveDataName = "NPID12345_BSAVEDATAEXAMPLE.PSW";
        public static Action<string> PostLog;

        /** Binary data saved here */
        private Dictionary<string, byte[]> binaryData = new Dictionary<string, byte[]>();

        /** This code may not compile with IL2CPP, use USE_CODEC_LOOKUP to use the type-codec pair lookup feature */
#if USE_CODEC_LOOKUP
        private Dictionary<Type, object> encoders = new Dictionary<Type, object>();
        private Dictionary<Type, object> decoders = new Dictionary<Type, object>();
#endif

        /** Constructor should only be called by instance getter when saved instance is null */
        private SaveData()
        {
            if (IsSaveFileExist())
            {
                ReadFile();
            }
            else
            {
                TrySaveFile();
                ReadFile();
            }
        }

#if USE_CODEC_LOOKUP
        #region Type-codec pair lookup database operations
        public static void SetEncoder<T>(BinaryEncoder<T> encoder)
        {
            var type = typeof(T);
            if (instance.encoders.ContainsKey(type)) instance.encoders[type] = encoder;
            else instance.encoders.Add(type, encoder);
        }
        
        public static void SetDecoder<T>(BinaryDecoder<T> decoder)
        {
            var type = typeof(T);
            if (instance.decoders.ContainsKey(type)) instance.decoders[type] = decoder;
            else instance.decoders.Add(type, decoder);
        }

        public static void RemoveEncoder(Type type)
        {
            if (instance.encoders.ContainsKey(type))
            {
                instance.encoders.Remove(type);
            }
        }

        public static void RemoveDecoder(Type type)
        {
            if (instance.decoders.ContainsKey(type))
            {
                instance.decoders.Remove(type);
            }
        }
        #endregion
#endif

        #region IO File Operations
        private void ReadFile()
        {
            using (FileStream file = File.OpenRead(Path.Combine(SaveDataPath, SaveDataName)))
            {
                try
                {
                    byte[] headerBuffer = new byte[header.Length];
                    int headerLength = file.Read(headerBuffer, 0, header.Length);
                    if (headerLength >= header.Length && header.Sequal(headerBuffer))
                    {
                        var data = file.MakeByteArrayFromHere();
                        binaryData = fileBytesDecoder(data);
                    }
                    else
                    {
                        throw new IOException("Header mismatch.");
                    }
                }
                catch (Exception e) { PostLog?.Invoke($"READ - {e.GetType().Name} : {e.Message}\n\n STACK TRACE : {e.StackTrace}"); }
            }
        }
        
        private void TrySaveFile()
        {
            using(FileStream file = File.Open(Path.Combine(SaveDataPath, SaveDataName), FileMode.OpenOrCreate))
            {
                try
                {
                    file.Write(header, 0, header.Length);
                    byte[] data = fileBytesEncoder(binaryData);
                    file.Write(data, 0, data.Length);
                    file.Flush();
                }
                catch (Exception e) { PostLog?.Invoke($"WRITE - {e.GetType().Name} : {e.Message}\n\n STACK TRACE : {e.StackTrace}"); }
            }
        }

        private bool IsSaveFileExist()
        {
            return File.Exists(Path.Combine(SaveDataPath, SaveDataName));
        }
#endregion

#region Static Load/Save methods
        public static void Load()
        {
            instance.ReadFile();
        }

        public static void Save()
        {
            instance.TrySaveFile();
        }
        #endregion

#if USE_CODEC_LOOKUP
        #region Get/Set with pre-added type-codec pair lookup (Should be manually added first)

        public static T Get<T>(string key, T val = default)
        {
            T retval = val;
            try
            {
                var t = typeof(T);
                if (instance.decoders.ContainsKey(t))
                {
                    object dec = instance.decoders[t];
                    if (dec is BinaryDecoder<T>)
                    {
                        byte[] binData = instance.binaryData[key];
                        retval = ((BinaryDecoder<T>)dec)(binData);
                    }
                    else
                    {
                        throw new FormatException($"The decoder of type {t.Name} is a {dec.GetType().Name} instead of BinaryDecoder<{t.Name}>, Please Re-set the decoder.");
                    }
                }
                else
                {
                    throw new KeyNotFoundException($"Unable to find decoder for type {t.Name}");
                }
            }
            catch (Exception e) { PostLog?.Invoke($"GET - {e.GetType().Name} : {e.Message}\n\n STACK TRACE: {e.StackTrace}"); }
            return retval;
        }

        public static T GetOrAdd<T>(string key, T defVal = default)
        {
            T retval = defVal;
            if (instance.binaryData.ContainsKey(key))
            {
                retval = Get(key, defVal);
            }
            else
            {
                Set(key, defVal);
            }
            return retval;
        }

        public static void Set<T>(string key, T value)
        {
            try
            {
                var t = typeof(T);
                if (instance.encoders.ContainsKey(t))
                {
                    object enc = instance.encoders[t];
                    if (enc is BinaryEncoder<T>)
                    {
                        byte[] retval = ((BinaryEncoder<T>)enc)(value);
                        if (instance.binaryData.ContainsKey(key))
                        {
                            instance.binaryData[key] = retval;
                        }
                        else
                        {
                            instance.binaryData.Add(key, retval);
                        }
                    }
                    else throw new FormatException($"The encoder of type {t.Name} is a {enc.GetType().Name} instead of BinaryEncoder<{t.Name}>, Please Re-set the encoder.");
                }
                else throw new KeyNotFoundException($"Unable to find encoder for type {t.Name}");
            }
            catch (Exception e) { PostLog?.Invoke($"SET - {e.GetType().Name} : {e.Message}\n\n STACK TRACE: {e.StackTrace}"); }
        }
        #endregion
#endif

        #region Get/Set with a specified codec which should be specified in every call
        /** Generic getter for saved data,  */
        public static T Get<T>(string key, BinaryDecoder<T> decoder, T defVal = default)
        {
            T retval = defVal;
            try
            {
                byte[] binData = instance.binaryData[key];
                retval = decoder(binData);
            }
            catch (Exception) { };
            return retval;
        }

        /** Generic getter for saved data, wil add the value to saved if the key is not found in data dictionary */
        public static T GetOrAdd<T>(string key, BinaryDecoder<T> decoder, T valueIfNone = default, BinaryEncoder<T> encoder = default)
        {
            T retval = valueIfNone;
            try
            {
                if (instance.binaryData.ContainsKey(key))
                {
                    byte[] binData = instance.binaryData[key];
                    retval = decoder(binData);
                }
                else
                {
                    byte[] binData = encoder(valueIfNone);
                    instance.binaryData.Add(key, binData);
                }
            }
            catch (Exception) { };
            return retval;
        }

        private static T GoA<T>(string key, BinaryDecoder<T> dec, T val, BinaryEncoder<T> enc) => GetOrAdd(key, dec, val, enc);

        /** Generic setter for saved data */
        public static void Set<T>(string key, BinaryEncoder<T> encoder, T value)
        {
            try
            {
                byte[] binData = encoder(value);
                if (instance.binaryData.ContainsKey(key))
                {
                    instance.binaryData[key] = binData;
                }
                else
                {
                    instance.binaryData.Add(key, binData);
                }
            }
            catch (Exception) { };
        }

#endregion

#region Basic data Get/Set with default basic codecs and Unicode LE codec for string
        public static int  GetInt(string key, int defVal = 0) => Get(key, BasicCodec.IntDec, defVal);
        public static int  GetOrAddInt(string key, int defVal = 0) => GoA(key, BasicCodec.IntDec, defVal, BasicCodec.IntEnc);
        public static void SetInt(string key, int defVal = 0) => Set(key, BasicCodec.IntEnc, defVal);

        public static long GetLong(string key, long defVal = 0L) => Get(key, BasicCodec.LongDec, defVal);
        public static long GetOrAddLong(string key, long defVal = 0L) => GoA(key, BasicCodec.LongDec, defVal, BasicCodec.LongEnc);
        public static void SetLong(string key, long defVal = 0L) => Set(key, BasicCodec.LongEnc, defVal);

        public static byte GetByte(string key, byte defVal = 0) => Get(key, BasicCodec.ByteDec, defVal);
        public static byte GetOrAddByte(string key, byte defVal = 0) => GoA(key, BasicCodec.ByteDec, defVal, BasicCodec.ByteEnc);
        public static void SetByte(string key, byte defVal = 0) => Set(key, BasicCodec.ByteEnc, defVal);

        public static float GetFloat(string key, float defVal = 0F) => Get(key, BasicCodec.FloatDec, defVal);
        public static float GetOrAddFloat(string key, float defVal = 0F) => GoA(key, BasicCodec.FloatDec, defVal, BasicCodec.FloatEnc);
        public static void  SetFloat(string key, float defVal = 0F) => Set(key, BasicCodec.FloatEnc, defVal);
        
        public static double GetDouble(string key, double defVal = 0) => Get(key, BasicCodec.DoubleDec, defVal);
        public static double GetOrAddDouble(string key, double defVal = 0) => GoA(key, BasicCodec.DoubleDec, defVal, BasicCodec.DoubleEnc);
        public static void   SetDouble(string key, double defVal = 0) => Set(key, BasicCodec.DoubleEnc, defVal);

        public static short GetShort(string key, short defVal = 0) => Get(key, BasicCodec.ShortDec, defVal);
        public static short GetOrAddShort(string key, short defVal = 0) => GoA(key, BasicCodec.ShortDec, defVal, BasicCodec.ShortEnc);
        public static void  SetShort(string key, short defVal = 0) => Set(key, BasicCodec.ShortEnc, defVal);

        public static uint GetUInt(string key, uint defVal = 0) => Get(key, BasicCodec.UIntDec, defVal);
        public static uint GetOrAddUInt(string key, uint defVal = 0) => GoA(key, BasicCodec.UIntDec, defVal, BasicCodec.UIntEnc);
        public static void SetUInt(string key, uint defVal = 0) => Set(key, BasicCodec.UIntEnc, defVal);

        public static ulong GetULong(string key, ulong defVal = 0L) => Get(key, BasicCodec.ULongDec, defVal);
        public static ulong GetOrAddULong(string key, ulong defVal = 0L) => GoA(key, BasicCodec.ULongDec, defVal, BasicCodec.ULongEnc);
        public static void  SetULong(string key, ulong defVal = 0L) => Set(key, BasicCodec.ULongEnc, defVal);
        
        public static ushort GetUShort(string key, ushort defVal = 0) => Get(key, BasicCodec.UShortDec, defVal);
        public static ushort GetOrAddUShort(string key, ushort defVal = 0) => GoA(key, BasicCodec.UShortDec, defVal, BasicCodec.UShortEnc);
        public static void   SetUShort(string key, ushort defVal = 0) => Set(key, BasicCodec.UShortEnc, defVal);

        public static string GetUString(string key, string defVal = "") => Get(key, StringCodec.UCSLEDec, defVal);
        public static string GetOrAddUString(string key, string defVal = "") => GoA(key, StringCodec.UCSLEDec, defVal, StringCodec.UCSLEEnc);
        public static void   SetUString(string key, string defVal = "") => Set(key, StringCodec.UCSLEEnc, defVal);
#endregion
    }
}