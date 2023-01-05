using Newtonsoft.Json;
using Mutagen.Bethesda.Json;
using System.IO;

namespace HunterbornExtender;

public class JSONhandler<T>
{
    public static JsonSerializerSettings GetCustomJSONSettings()
    {
        var jsonSettings = new JsonSerializerSettings();
        jsonSettings.AddMutagenConverters();
        jsonSettings.ObjectCreationHandling = ObjectCreationHandling.Replace;
        jsonSettings.Formatting = Formatting.Indented;
        jsonSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter()); // https://stackoverflow.com/questions/2441290/javascriptserializer-json-serialization-of-enum-as-string

        return jsonSettings;
    }

    public static T? Deserialize(string jsonInputStr, out string errorString)
    {
        errorString = "";
        try
        {
            return JsonConvert.DeserializeObject<T>(jsonInputStr, GetCustomJSONSettings());
        }
        catch (Exception ex)
        {
            // log
            errorString = ExceptionRecorder.GetExceptionStack(ex, "");
            //MessageBox.Show(error);
            return default;
        }
    }

    public static T? LoadJSONFile(string loadLoc, out string errorString)
    {
        return Deserialize(File.ReadAllText(loadLoc), out errorString);
    }

    public static string Serialize(T input)
    {
        return JsonConvert.SerializeObject(input, Formatting.Indented, GetCustomJSONSettings());
    }

    public static void SaveJSONFile(T input, string saveLoc)
    {
        if (Path.GetDirectoryName(saveLoc) is string dir && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        File.WriteAllText(saveLoc, Serialize(input));
    }
}