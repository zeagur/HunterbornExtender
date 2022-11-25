using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Mutagen.Bethesda.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Windows;
using HunterbornExtenderUI;

namespace SynthEBD
{
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

        public static T? Deserialize(string jsonInputStr)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(jsonInputStr, GetCustomJSONSettings());
            }
            catch (Exception ex)
            {
                // log
                string error = ExceptionRecorder.GetExceptionStack(ex, "");
                //MessageBox.Show(error);
                return default;
            }
        }

        public static T? LoadJSONFile(string loadLoc)
        {
            return Deserialize(File.ReadAllText(loadLoc));
        }

        public static string Serialize(T input)
        {
            return JsonConvert.SerializeObject(input, Formatting.Indented, GetCustomJSONSettings());
        }

        public static void SaveJSONFile(T input, string saveLoc)
        {
            File.WriteAllText(saveLoc, Serialize(input));
        }
    }

}
