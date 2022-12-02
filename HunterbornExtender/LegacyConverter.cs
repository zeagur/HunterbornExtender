using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Noggog;
using System.Text.Json;
using HunterbornExtenderUI;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda;
using SynthEBD;
using Mutagen.Bethesda.Plugins.Exceptions;
using static HunterbornExtender.LegacyConverter;
using ReactiveUI;
using System.Collections;

namespace HunterbornExtender
{
    sealed public class LegacyConverter
    {
        static public List<PluginEntry> ImportAndConvert(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            // LOAD JSONS
            Console.WriteLine($"\tCurrent directory: {Directory.GetCurrentDirectory()}");
            Directory.SetCurrentDirectory("c:\\users\\Mark\\source\\repos\\HunterbornExtender\\HunterbornExtender\\Zedit");
            Console.WriteLine($"\tChanged directory to: {Directory.GetCurrentDirectory()}");

            List<PluginEntry> plugins = new();
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

        static List<PluginEntry> ReadFile(String fileName, IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            string jsonString = File.ReadAllText(fileName);
            Console.WriteLine($"\t\t o Successfully read {fileName}");

            var jsonLegacy = JsonSerializer.Deserialize<List<HBJsonDataLegacy>>(jsonString);
            if (jsonLegacy == null)
            {
                Console.WriteLine($"\t\t x Deserialization failed.");
                return new();
            }

            List<PluginEntry> plugins = new();
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

            public PluginEntry ToPlugin(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
            {
                var materials = mats.Select(lvl => {
                    Dictionary<IFormLinkGetter<IItemGetter>, int> Materials_Level = new();
                    foreach (var lvl_mats in lvl)
                    {
                        var item = state.LinkCache.Resolve<IItemGetter>(lvl_mats.Key).ToLink();
                        Materials_Level[item] = lvl_mats.Value;
                    }
                    return Materials_Level;
                }).ToList();

                PluginEntry plugin = new(type.ContainsInsensitive("anim") ? EntryType.Animal : EntryType.Monster, name);
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

    sealed public class PluginEntry
    {
        public EntryType Type { get; set; } = EntryType.Animal;
        public String Name { get; set; } = "Critter";
        public String ProperName { get; set; } = "Critter";
        public String SortName { get; set; } = "Critter";
        public IFormLinkGetter<IGlobalGetter> Toggle { get; set; } = new FormLink<IGlobalGetter>();
        public IFormLinkGetter<IMessageGetter> CarcassMessageBox { get; set; } = new FormLink<IMessageGetter>();
        public IFormLinkGetter<IItemGetter> Meat { get; set; } = new FormLink<IItemGetter>();
        public int CarcassSize { get; set; } = 1;
        public int CarcassWeight { get; set; } = 10;
        public int CarcassValue { get; set; } = 10;
        public int[] PeltCount { get; set; } = new int[] { 1, 1, 1, 1 };
        public int[] FurPlateCount { get; set; } = new int[] { 1, 1, 1, 1 };
        public List<Dictionary<IFormLinkGetter<IItemGetter>, int>> Materials { get; set; } = new ();
        public List<IFormLinkGetter<IItemGetter>> Discard { get; set; } = new();
        public IFormLinkGetter<IFormListGetter> SharedDeathItems { get; set; } = new FormLink<IFormListGetter>();
        public IFormLinkGetter<IItemGetter> BloodType { get; set; } = new FormLink<IItemGetter>();
        public IFormLinkGetter<IItemGetter> Venom { get; set; } = new FormLink<IItemGetter>();
        public IFormLinkGetter<IVoiceTypeGetter> Voice { get; set; } = new FormLink<IVoiceTypeGetter>();

        public PluginEntry(EntryType type, string name) {
            Type = type;
            Name = name;
        }

        public PluginEntry(EntryType type, string name, string properName, string sortName, IFormLinkGetter<IGlobalGetter> toggle, IFormLinkGetter<IMessageGetter> carcassMessageBox, IFormLinkGetter<IItemGetter> meat, int carcassSize, int carcassWeight, int carcassValue, int[] peltCount, int[] furPlateCount, List<Dictionary<IFormLinkGetter<IItemGetter>, int>> materials, List<IFormLinkGetter<IItemGetter>> discard, IFormLinkGetter<IFormListGetter> sharedDeathItems, IFormLinkGetter<IItemGetter> bloodType, IFormLinkGetter<IItemGetter> venom, IFormLinkGetter<IVoiceTypeGetter> voice)
        {
            Type = type;
            Name = name;
            ProperName = properName;
            SortName = sortName;
            Toggle = toggle;
            CarcassMessageBox = carcassMessageBox;
            Meat = meat;
            CarcassSize = carcassSize;
            CarcassWeight = carcassWeight;
            CarcassValue = carcassValue;
            PeltCount = peltCount;
            FurPlateCount = furPlateCount;
            Materials = materials;
            Discard = discard;
            SharedDeathItems = sharedDeathItems;
            BloodType = bloodType;
            Venom = venom;
            Voice = voice;
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
