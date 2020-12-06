// This file shouldn't do anything outside Unity environment
// the PSW_BINARYSAVE_UNITY_TEST_ENABLED symbol should only be defined from Unity
// through Player Settings -> Other Settings -> Compilation Symbols

#if PSW_BINARYSAVE_UNITY_TEST_ENABLED
using UnityEngine;
using PSW.BinarySave;

[AddComponentMenu("PSW/Binary Save Test Component")]
public class BinarySaveTest : MonoBehaviour
{

// This class won't do anything when compiled outside editor
#if UNITY_EDITOR
    // Start is called before the first frame update
    void Start()
    {
        SaveData.SaveDataPath = Application.persistentDataPath;
        SaveData.PostLog = (str) => { Debug.Log(str); };
        SaveData.SetInt("TEST_INT_MAXVALUE", int.MaxValue);
        SaveData.SetInt("TEST_INT_MINVALUE", int.MinValue);
        SaveData.SetFloat("TEST_FLOAT_MAXVALUE", float.MaxValue);
        SaveData.SetFloat("TEST_FLOAT_MINVALUE", float.MinValue);
        SaveData.SetLong("TEST_LONG_MAXVALUE", long.MaxValue);
        SaveData.SetLong("TEST_LONG_MINVALUE", long.MinValue);
        SaveData.Set("TEST_STRING_UTF32", StringCodec.UTF32Enc, "UTF32");
        SaveData.Set("TEST_STRING_UTF7", StringCodec.UTF7Enc, "UTF7");
        SaveData.Set("TEST_STRING_UTF8", StringCodec.UTF8Enc, "UTF8");
        SaveData.Set("TEST_STRING_ASCII", StringCodec.ASCIIEnc, "ASCII");
        SaveData.Set("TEST_STRING_UCSLE", StringCodec.UCSLEEnc, "UCSLE");
        SaveData.Set("TEST_STRING_UCSBE", StringCodec.UCSBEEnc, "UCSBE");
        SaveData.Save();
    }
#endif
}
#endif