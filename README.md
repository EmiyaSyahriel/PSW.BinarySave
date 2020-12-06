# PSW.BinarySave
Simple and customizable semi low-level IO interface for dictionary-based game save data library. Designed for Unity.

Tested with Unity 2020.1.4f1.

## Adding to Your Project
### Unity
Put the entire project (excluding the vsproj) to anywhere in the `Assets`.

Alternatively, you can download the precompiled DLL from release page and put the DLL to your `Assets\Plugins` folder
### Non-Unity
Include the Project (or the `.vsproj`) to your solution (Solution > Add > Existing Project).

## Usage
First, Set the File path and name before your first access to SaveData Get/Set method. This class will initialize itself once when Get/Set method is called and then cache
itself, so you can only be able to set the file path and name once before accessing the Get/Set method.
```
  using PSW.BinarySave;
  using System;
  
  // Save the file at Unity Persistent data path
  SaveData.SaveFilePath = Application.persistentDataPath;
  // Or at current execution directory
  SaveData.SaveFilePath = Environment.CurrentDirectory;
  // Or at Executable location
  SaveData.SaveFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
  
  // Set the file name, any type is accepted
  SaveData.SaveFileName = "GameSave0.dat";
```
Then, you have to define some 'Codec's that converts Byte Array into runtime data and vice versa, these Codecs has type of BinaryDecoder and BinaryEncoder.
Some codec were already defined in BasicCodec static class.
```
  using PSW.BinarySave;
  using System.Text;
  
  // A Simple string codec
  // Decoder decodes byte array into runtime data
  BinaryDecoder<string> stringDecoder = (byteArray) => { Encoding.ASCII.GetString(byteArray); };
  // Encoder encodes runtime data into byte array
  BinaryEncoder<string> stringEncoder = (source) => { Encoding.ASCII.GetBytes(source); };
```
Then, use the codec to access the SaveData using data key
```
  using PSW.BinarySave;
  
  // First access won't need any load, only in case you wanted to reload
  SaveData.Load();
  
  SaveData.Set("SAVING_KEY_WHATEVER_YOU_WANT", stringEncoder, "The string I wanted to set");
  SaveData.Get("READING_KEY_WHATEVER_I_WANT", stringDecoder);
  SaveData.GetOrAdd("THIS_KEY_MIGHT_NOT_AVAILABLE", stringDecoder, "In case it's not already defined in save file", stringEncoder);
  
  // Predefineds:
  SaveData.SetInt("AnInteger", 0);
  SaveData.SetLong("int64 or long", 0L);
  
  // Write your data to disk
  SaveData.Save();
```
You can also add the codec beforehand as a lookup and you won't need to specify which codec to call everytime. to use this feature, 
Add `USE_CODEC_LOOKUP` to your conditional compilation symbol (available by default on prebuild package).
```
  // These codecs will be kept during runtime, you only need to add it once during initialization
  SaveData.AddEncoder(stringEncoder);
  SaveData.AddDecoder(stringDecoder);
  
  // The Get/Set method will find which codec to use using the type of the data given as param
  SaveData.Set("aLookupTest", "");
  SaveData.Get<string>("GET LOOKUPTest");
```
You can also control on how the data contained in the SaveData class is serialized and deserialized into file by setting the `SaveData.fileBytesDecoder` and `SaveData.fileBytesEncoder`. The default is using the `RLCTFormat.Decoder` and `RLCTFormat.Encoder` which reads/writes the data as a simple TOC-then-data file format similar to GRAF AFS.

## Reference Compilation
If you do not want to edit the source code of this library after cloning or just edit once for all, you can compile this library as a normal DLL.

Open the `PSW.BinarySave.vsproj` file with Visual Studio or any C#-capable IDE and then Build it with `Release|Any CPU` configuration.
and then add the built DLL as reference to your C# Project / Solution.

### Framework Version
This project uses .NET Standard 2.0 to comply with Unity 2020.1.4f1.
But, you can change the target to any Framework that support C# v7.2 or later and contains these modules:
- `System`
- `System.IO`
- `System.Linq`
- `System.Text`
- `System.Collections.Generic`

## License
MIT License.
