using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Noggog;
using System.Text.Json;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins.Exceptions;
using HunterbornExtender.Settings;

namespace HunterbornExtender
{
    sealed public class LegacyConverter
    {
        static public List<AddonPluginEntry> ImportAndConvert(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            // LOAD JSONS
            Console.WriteLine($"\tCurrent directory: {Directory.GetCurrentDirectory()}");
            Directory.SetCurrentDirectory("c:\\users\\Mark\\source\\repos\\HunterbornExtender\\HunterbornExtender\\Zedit");
            Console.WriteLine($"\tChanged directory to: {Directory.GetCurrentDirectory()}");

            List<AddonPluginEntry> plugins = new();
            var serializationOptions = new JsonSerializerOptions { WriteIndented = true };

            foreach (var fileName in Directory.EnumerateFiles(".", "*.json"))
            {
                try
                {
                    Console.WriteLine($"\tReading legacy zedit plugin set: ${fileName}");
                    var filePlugins = ReadFile(fileName, state);
                    plugins.AddRange(filePlugins);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine(ex.StackTrace);
                    break;
                }
            }

            return plugins;
        }

        static List<AddonPluginEntry> ReadFile(String fileName, IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            string jsonString = File.ReadAllText(fileName);
            Console.WriteLine($"\t\t o Successfully read {fileName}");

            var jsonLegacy = JsonSerializer.Deserialize<List<HBJsonDataLegacy>>(jsonString);
            if (jsonLegacy == null)
            {
                Console.WriteLine($"\t\t x Deserialization failed.");
                return new();
            }

            List<AddonPluginEntry> plugins = new();
            foreach (var legacy in jsonLegacy)
            {
                try
                {
                    var plugin = legacy.ToPlugin(state);
                    plugins.Add(plugin);
                    Console.WriteLine($"\t\t\t o Added plugin for {plugin.Name}.");
    
                    /*
                    if (legacy.name.ToLower().Equals("giant"))
                    {
                        TestImportConversion(legacy, plugin);
                    }*/
                }
                catch (MissingRecordException ex)
                {
                    Console.WriteLine($"\t\t\t x Conversion of [{ex.EditorID}] from \"{legacy.name}\" in {fileName} to Mutagen failed -- record missing.");
                    //Console.WriteLine(ex.Message);
                    //Console.WriteLine(ex.StackTrace);
                    continue;
                }
                catch (RecordException ex)
                {
                    Console.WriteLine("========================");
                    Console.WriteLine($"\t\t\t x Conversion of [{ex.EditorID}] from \"{legacy.name}\" in {fileName} to Mutagen failed.");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    //Console.WriteLine(JSONhandler<HBJsonDataLegacy>.Serialize(jsonLegacy[0]));
                    Console.WriteLine("========================");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("========================");
                    Console.WriteLine($"\t\t\t x Conversion of \"{legacy.name}\" to Mutagen failed: {ex.Message}.");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    Console.WriteLine("========================");
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

            public string[] peltCount { get; set; } = new string[] { "1", "1", "1", "1" };

            public string[] furPlateCount { get; set; } = new string[] { "1", "1", "1", "1" };
            public string? meat { get; set; }
            public string[] negativeTreasure { get; set; } = new string[0];
            public string? sharedDeathItems { get; set; }
            public string? bloodType { get; set; }
            public string? venom { get; set; }
            public string? voice { get; set; }
            public List<Dictionary<string, int>> mats { get; set; } = new();

            public AddonPluginEntry ToPlugin(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
            {
                var materials = mats.Select(lvl => {
                    Dictionary<IFormLinkGetter<IItemGetter>, int> Materials_Level = new();
                    foreach (var lvl_mats in lvl)
                    {
                        var item = state.LinkCache.Resolve<IItemGetter>(lvl_mats.Key).ToLink();
                        Materials_Level[item] = lvl_mats.Value;
                    }
                    return new MaterialLevel() { Items = Materials_Level };
                }).ToList();

                AddonPluginEntry plugin = new(type.ContainsInsensitive("anim") ? EntryType.Animal : EntryType.Monster, name);
                plugin.ProperName = properName ?? name;
                plugin.SortName = sortName ?? name;
                plugin.Toggle = animalSwitch.IsNullOrWhitespace() ? new FormLink<IGlobalGetter>() : state.LinkCache.Resolve<IGlobalGetter>(animalSwitch).ToLink();
                plugin.CarcassMessageBox = carcassMessageBox.IsNullOrWhitespace() ? new FormLink<IMessageGetter>() : state.LinkCache.Resolve<IMessageGetter>(carcassMessageBox).ToLink();
                plugin.Meat = meat.IsNullOrWhitespace() ? new FormLink<IItemGetter>() : state.LinkCache.Resolve<IItemGetter>(meat).ToLink();
                plugin.CarcassSize = int.TryParse(carcassSize, out var sz) ? sz : 1;
                plugin.CarcassWeight = int.TryParse(carcassWeight, out var wt) ? wt : 10;
                plugin.CarcassValue = int.TryParse(carcassValue, out var val) ? val : 10;
                plugin.PeltCount = peltCount.Select(s => int.TryParse(s, out var c) ? c : 1).ToArray();
                plugin.FurPlateCount = furPlateCount.Select(s => int.TryParse(s, out var c) ? c : 1).ToArray();
                plugin.Materials = materials;
                plugin.Discard = negativeTreasure.Select(s => state.LinkCache.Resolve<IItemGetter>(s).ToLink() as IFormLinkGetter<IItemGetter>).ToList();
                plugin.SharedDeathItems = sharedDeathItems.IsNullOrWhitespace() ? new FormLink<IFormListGetter>() : state.LinkCache.Resolve<IFormListGetter>(sharedDeathItems).ToLink();
                plugin.BloodType = bloodType.IsNullOrWhitespace() ? new FormLink<IItemGetter>() : state.LinkCache.Resolve<IItemGetter>(bloodType).ToLink();
                plugin.Venom = venom.IsNullOrWhitespace() ? new FormLink<IIngestibleGetter>() : state.LinkCache.Resolve<IIngestibleGetter>(venom).ToLink();
                plugin.Voice = voice.IsNullOrWhitespace() ? new FormLink<IVoiceTypeGetter>() : state.LinkCache.Resolve<IVoiceTypeGetter>(voice).ToLink();
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

        static public void TestImportConversion(HBJsonDataLegacy legacy, PluginEntry plugin)
        {
            List<Dictionary<String, int>> expected = new() { 
                new() {},
                new() {{ "GiantToes", 1} },
                new() {{ "GiantToes", 2} },
                new() {}
            };

            TestField("Name", "Giant", legacy.name, plugin.Name);
            TestField("Type", "Monster", legacy.type, plugin.Type.ToString());
            TestField("CarcassSize", "5", legacy.carcassSize, plugin.CarcassSize.ToString());
            TestField("PeltCount", "[4,2,2,2]", legacy.peltCount.PPrint(), plugin.PeltCount.PPrint());
            TestField("FurPlateCount", "[2,4,8,16]", legacy.furPlateCount.PPrint(), plugin.FurPlateCount.PPrint());
            TestField("Materials", expected.PPrint(), legacy.mats.PPrint(), plugin.Materials.PPrint());
            TestField("Discards", "[GiantToes]", legacy.negativeTreasure.PPrint(), plugin.Discard.PPrint());
            TestField("Voice", "[CrGiantVoice]", legacy.voice, plugin.Voice.ToString());
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
