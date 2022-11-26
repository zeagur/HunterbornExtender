using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HunterbornExtender
{
    internal class LegacyConverter
    {
        static void convert()
        {
            // LOAD JSONS
            Console.WriteLine($"Current Directory: {Directory.GetCurrentDirectory().ToString()}");

            Directory.SetCurrentDirectory("..\\..\\..\\Legacy");
            string modernPatchesFolder = "..\\addons";

            if (!Directory.Exists(modernPatchesFolder))
            {
                Directory.CreateDirectory(modernPatchesFolder);
            }

            Directory.EnumerateFiles(".", "*.json").Where(fileName => !fileName.ContainsInsensitive("module.json")).ForEach(fileName =>
            {
                try
                {
                    string jsonString = File.ReadAllText(fileName);
                    Console.WriteLine($"Successfully read {fileName}");
                    var jsonLegacy = JsonSerializer.Deserialize<HBJsonDataLegacy[]>(jsonString);
                    List<HBJsonDataModern> jsonModern = new();

                    if (jsonLegacy != null)
                    {
                        foreach (var legacy in jsonLegacy)
                        {
                            try
                            {
                                int[] pelts = legacy.peltCount.Select(c => int.Parse(c)).ToArray();
                                int[] furs = legacy.furPlateCount.Select(c => int.Parse(c)).ToArray();

                                Dictionary<IItemGetter, int>[] mats = legacy.mats.Select(mat =>
                                {
                                    Dictionary<IItemGetter, int> m2 = new();
                                    foreach (var key in mat.Keys)
                                    {
                                        var item = GetItem(key, state);
                                        if (item != null) m2[item] = mat[key];
                                    }
                                    return m2;

                                }).ToArray();

                                int size = 1;
                                try
                                {
                                    size = int.Parse(legacy.carcassSize);
                                }
                                catch (Exception ex)
                                {
                                    if (!legacy.carcassSize.IsNullOrWhitespace())
                                        Console.WriteLine($"Invalid SIZE field for {legacy.name}: {legacy.carcassValue}");
                                }
                                int weight = size * 10;
                                try
                                {
                                    weight = int.Parse(legacy.carcassWeight);
                                }
                                catch (Exception ex)
                                {
                                    if (!legacy.carcassWeight.IsNullOrWhitespace())
                                        Console.WriteLine($"Invalid WEIGHT field for {legacy.name}: {legacy.carcassValue}");
                                }

                                int value = size * 5;
                                try
                                {
                                    value = int.Parse(legacy.carcassValue);
                                }
                                catch (Exception ex)
                                {
                                    if (!legacy.carcassValue.IsNullOrWhitespace())
                                        Console.WriteLine($"Invalid VALUE field for {legacy.name}: {legacy.carcassValue}");
                                }

                                var modern = new HBJsonDataModern(
                                    legacy.type.ContainsInsensitive("monster") ? CreatureClass.Monster : CreatureClass.Animal,
                                    legacy.name, legacy.properName, legacy.sortName,
                                    GetEdid<Global>(legacy.animalSwitch, state),
                                    GetEdid<Message>(legacy.carcassMessageBox, state),
                                    size, weight, value,
                                    new PeltCount(pelts[0], pelts[1], pelts[2], pelts[3]),
                                    new FurCount(pelts[0], pelts[1], pelts[2], pelts[3]),
                                    GetItem(legacy.meat, state),
                                    new Materials(mats[0], mats[1], mats[2], mats[3]),
                                    legacy.negativeTreasure.Select(edid => GetItem(edid, state)).Where(item => item != null).Select(item => item!).ToArray(),
                                    legacy.sharedDeathItems,
                                    GetItem(legacy.bloodType, state),
                                    GetEdid<Ingestible>(legacy.venom, state),
                                    GetEdid<VoiceType>(legacy.voice, state)
                                    );

                                //Console.WriteLine(modern);
                                jsonModern.Add(modern);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"{fileName} : {ex.ToString()}");
                            }
                        }
                    }

                    var options = new JsonSerializerOptions { WriteIndented = true };
                    var jsonOutput = JsonSerializer.Serialize(jsonModern, options);

                    string outputFilename = $"{modernPatchesFolder}\\{fileName}";
                    Console.WriteLine($"Writing to {outputFilename}");
                    //File.CreateText(outputFilename);
                    File.WriteAllText(outputFilename, jsonOutput);

                    Console.WriteLine("=============");

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            });


        }

        record HBJsonDataLegacy(string type, string name, string properName, string sortName, string animalSwitch, string carcassMessageBox,
            string carcassSize, string carcassWeight, string carcassValue, string[] peltCount, string[] furPlateCount,
            string meat, string[] negativeTreasure, string sharedDeathItems, string bloodType, string venom, string voice, Dictionary<string, int>[] mats);


        static T? GetEdid<T>(string? edid, IPatcherState<ISkyrimMod, ISkyrimModGetter> state) where T : SkyrimMajorRecord
        {
            if (edid == null) return default;
            state.LinkCache.TryResolve<T>(edid, out var form);
            //new EDIDLink<T>(edid).TryResolve(state.LinkCache, out var form);
            return form;
        }

        static IItem? GetItem(string? edid, IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            var f = GetEdid<SkyrimMajorRecord>(edid, state);
            return f as IItem;
        }

    }


}
