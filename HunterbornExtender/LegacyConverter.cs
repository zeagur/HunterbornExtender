using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Noggog;
using System.Text.Json;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins.Exceptions;
using HunterbornExtender.Settings;
using Mutagen.Bethesda.Plugins.Cache;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using DynamicData;
using System.IO.Abstractions;
//using Newtonsoft.Json;

namespace HunterbornExtender
{
    sealed public class LegacyConverter
    {
        static public List<AddonPluginEntry> ImportAndConvert(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            List<string> directoriesToTry = new();
            var x = new DirectoryPath("");
            directoriesToTry.AddRange(CheckPath("Game Data", $"{state.DataFolderPath}\\skse\\plugins\\HunterbornExtender"));
            directoriesToTry.AddRange(CheckPath("Settings Data Path", $"{state.ExtraSettingsDataPath}"));
            directoriesToTry.AddRange(CheckPath("Internal Data Path", $"{state.InternalDataPath}\\Plugins"));

            HashSet<string> previousFilenames = new();
            List<AddonPluginEntry> plugins = new();
            new JsonSerializerOptions { WriteIndented = true };

            foreach (var path in directoriesToTry)
            {
                if (Directory.Exists(path))
                {
                    Directory.SetCurrentDirectory(path);
                    Write.Action(1, $"Changed directory to: {Directory.GetCurrentDirectory()}");
                    var filePaths = Directory
                        .EnumerateFiles(path, "*.json")
                        .Where(path => !path.ContainsInsensitive("settings.json"))
                        .ToList();

                    if (filePaths.Count > 0) Write.Success(1, $"Found {filePaths.Count} json files.");
                    else Write.Fail(1, $"No json files found.");
                    Write.Action(0, $"Previous filenames: {previousFilenames.Pretty()}");

                    foreach (var filePath in filePaths)
                    {
                        var filename = Path.GetFileName(filePath).ToLower();

                        if (filePath is not null && !previousFilenames.Contains(filename))
                        {
                            try
                            {
                                Write.Action(1, $"Reading zedit plugin set: {filePath}");
                                var filePlugins = ReadFile(filePath, state.LinkCache);
                                plugins.AddRange(filePlugins);
                                previousFilenames.Add(filename);
                            }
                            catch (Exception ex)
                            {
                                Write.Fail(0, ex.ToString());
                                //break;
                            }
                        }
                    }
                }
            }

            return plugins;
        }

        static IEnumerable<string> CheckPath(string name, DirectoryPath? directory)
        {           
            if (directory is not null && Directory.Exists(directory))
            {
                Write.Success(1, $"{name}: {directory} (EXISTS)");
                return new List<string>() { directory };
            }
            else
            {
                Write.Action(1, $"{name}: {directory} (MISSING))");
                return new List<string>();
            }


        }

        static List<AddonPluginEntry> ReadFile(String fileName, ILinkCache<ISkyrimMod, ISkyrimModGetter> linkCache)
        {
            string jsonString = File.ReadAllText(fileName);
            //Write.Success(2, $"Successfully read {fileName}");

            //JsonConvert.DeserializeObject<Settings.Settings>(jsonInputStr, GetCustomJSONSettings());
            var jsonPlugin = JsonSerializer.Deserialize<List<AddonPluginEntry>>(jsonString);
            if (jsonPlugin is List<AddonPluginEntry> importedPlugins) return importedPlugins;

            var jsonLegacy = JsonSerializer.Deserialize<List<HBJsonDataLegacy>>(jsonString);
            if (jsonLegacy == null)
            {
                Write.Fail(2, $"Deserialization failed.");
                return new();
            }

            List<AddonPluginEntry> plugins = new();
            foreach (var legacy in jsonLegacy)
            {
                try
                {
                    var plugin = legacy.ToPlugin(linkCache);
                    plugins.Add(plugin);
                    Write.Success(3, $"Added plugin for {plugin.Name} from {fileName}.");
   
                    //if (legacy.name.ToLower().ContainsInsensitive("hagraven")) TestImportConversion(legacy, plugin);
                }
                catch (MissingRecordException ex)
                {
                    Write.Fail(3, $"Conversion of [{ex.EditorID}] from \"{legacy.name}\" in {fileName} to Mutagen failed -- record missing.");
                    continue;
                }
                catch (RecordException ex)
                {
                    Write.Divider(0);
                    Write.Fail(3, $"Conversion of [{ex.EditorID}] from \"{legacy.name}\" in {fileName} to Mutagen failed.");
                    Write.Fail(0, ex.ToString());
                    Write.Divider(0);
                }
                catch (Exception ex)
                {
                    Write.Divider(0);
                    Write.Fail(3, $"Conversion of \"{legacy.name}\" in {fileName} to Mutagen failed.");
                    Write.Fail(0, ex.ToString());
                    Write.Divider(0);
                }
            }

            return plugins;
        }

        static readonly public int[] DefaultCounts = new int[] { 1, 1, 1, 1 };

        sealed public class HBJsonDataLegacy
        {
            public string type { get; set; } = EntryType.Animal.ToString();
            public string name { get; set; } = "Creechore";
            public string properName { get; set; } = "Creechore";
            public string sortName { get; set; } = "Creechore";
            public string? animalSwitch { get; set; }
            public string? carcassMessageBox { get; set; }
            public string carcassSize { get; set; } = "1";
            public string carcassWeight { get; set; } = "10";
            public string carcassValue { get; set; } = "10";

            public string[] peltCount { get; set; } = new string[] { "2", "2", "2", "2" };

            public string[] furPlateCount { get; set; } = new string[] { "1", "2", "4", "8" };
            public string? meat { get; set; }
            public string[] negativeTreasure { get; set; } = new string[0];
            public string? sharedDeathItems { get; set; }
            public string? bloodType { get; set; }
            public string? venom { get; set; }
            public string? voice { get; set; }
            public List<Dictionary<string, int>> mats { get; set; } = new();

            public AddonPluginEntry ToPlugin(ILinkCache<ISkyrimMod, ISkyrimModGetter> linkCache)
            {
                var materials = mats.Select(lvl => {
                    Dictionary<IFormLinkGetter<IItemGetter>, int> Materials_Level = new();
                    foreach (var lvl_mats in lvl)
                    {
                        var item = linkCache.Resolve<IItemGetter>(lvl_mats.Key).ToLink();
                        Materials_Level[item] = lvl_mats.Value;
                    }
                    return new MaterialLevel() { Items = Materials_Level };
                }).ToList();

                AddonPluginEntry plugin = new(type.ContainsInsensitive("anim") ? EntryType.Animal : EntryType.Monster, name);
                plugin.ProperName = properName ?? name;
                plugin.SortName = sortName ?? name;
                plugin.Toggle = animalSwitch.IsNullOrWhitespace() ? new FormLink<IGlobalGetter>() : linkCache.Resolve<IGlobalGetter>(animalSwitch).ToLink();
                plugin.CarcassMessageBox = carcassMessageBox.IsNullOrWhitespace() ? new FormLink<IMessageGetter>() : linkCache.Resolve<IMessageGetter>(carcassMessageBox).ToLink();
                plugin.Meat = meat.IsNullOrWhitespace() ? new FormLink<IItemGetter>() : linkCache.Resolve<IItemGetter>(meat).ToLink();
                plugin.CarcassSize = int.TryParse(carcassSize, out var sz) ? sz : 1;
                plugin.CarcassWeight = int.TryParse(carcassWeight, out var wt) ? wt : 10;
                plugin.CarcassValue = int.TryParse(carcassValue, out var val) ? val : 10;
                plugin.PeltCount = peltCount.Select(s => int.TryParse(s, out var c) ? c : 1).ToArray();
                plugin.FurPlateCount = furPlateCount.Select(s => int.TryParse(s, out var c) ? c : 1).ToArray();
                plugin.Materials = materials;
                plugin.Discard = negativeTreasure.Select(s => linkCache.Resolve<IItemGetter>(s).ToLink() as IFormLinkGetter<IItemGetter>).ToList();
                plugin.SharedDeathItems = sharedDeathItems.IsNullOrWhitespace() ? new FormLink<IFormListGetter>() : linkCache.Resolve<IFormListGetter>(sharedDeathItems).ToLink();
                plugin.BloodType = bloodType.IsNullOrWhitespace() ? new FormLink<IItemGetter>() : linkCache.Resolve<IItemGetter>(bloodType).ToLink();
                plugin.Venom = venom.IsNullOrWhitespace() ? new FormLink<IIngestibleGetter>() : linkCache.Resolve<IIngestibleGetter>(venom).ToLink();
                plugin.Voice = voice.IsNullOrWhitespace() ? new FormLink<IVoiceTypeGetter>() : linkCache.Resolve<IVoiceTypeGetter>(voice).ToLink();
                return plugin;
            }
        }

        static public string print<T>(List<T> list) where T : notnull
        {
            var printed = list.Select(i => print(i));
            var joined = String.Join(", ", printed);
            return $"[{joined}]";
        }

        static public string print<T>(T[] list) where T : notnull
        {
            var printed = list.Select(i => print(i));
            var joined = String.Join(", ", printed);
            return $"[{joined}]";
        }

        static public string print<S, T>(Dictionary<S, T> dict) where S : notnull
        {
            var printed = dict.Select(i => print(i));
            var joined = String.Join(", ", printed);
            return $"{{{joined}}}";
        }

        static public string print(object o) 
        {
            return $"{o}";
        }

        /*public static JsonSerializerSettings GetCustomJSONSettings()
        {
            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.AddMutagenConverters();
            jsonSettings.ObjectCreationHandling = ObjectCreationHandling.Replace;
            jsonSettings.Formatting = Formatting.Indented;
            jsonSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter()); // https://stackoverflow.com/questions/2441290/javascriptserializer-json-serialization-of-enum-as-string

            return jsonSettings;
        }*/

        static public void TestImportConversion(HBJsonDataLegacy legacy, PluginEntry plugin)
        {
            List<Dictionary<String, int>> expected = new() { 
                new() {},
                new() {}
            };

            TestField("Name", "Hagraven", legacy.name, plugin.Name);
            TestField("Type", "Monster", legacy.type, plugin.Type.ToString());
            TestField("CarcassSize", "5", legacy.carcassSize, plugin.CarcassSize.ToString());
            TestField("PeltCount", "[4,2,2,2]", legacy.peltCount.PPrint(), plugin.PeltCount.PPrint());
            TestField("FurPlateCount", "[2,4,8,16]", legacy.furPlateCount.PPrint(), plugin.FurPlateCount.PPrint());
            TestField("Materials", expected.PPrint(), legacy.mats.PPrint(), plugin.Materials.PPrint());
            TestField("Discards", "[]", legacy.negativeTreasure.PPrint(), plugin.Discard.PPrint());
            TestField("Voice", "[]", legacy.voice, plugin.Voice.ToString());
        }

        static void TestField(String field, String? expected, String? legacy, String? plugin)
        {
            Console.WriteLine($"{field,-8}   Expected: {expected,-12}  Legacy {legacy,-12}  Plugin {plugin,-12}");
        }

    }

    public static class GenericToStringExts
    {
        public static string PPrint<T>(this T[] array) where T : notnull => "[" + string.Join(", ", array) + "]";

        public static string PPrint<T>(this List<T> list) where T : notnull => "[" + string.Join(", ", list) + "]";

        public static string PPrint<S, T>(this Dictionary<S, T> dict) where S : notnull => "{" + string.Join(", ", dict) + "}";

        public static string PPrint<S,T>(this List<Dictionary<S,T>> listOfDicts) where S : notnull => "[" + string.Join(", ", listOfDicts.Select(l => l.PPrint())) + "]";

    }

}
