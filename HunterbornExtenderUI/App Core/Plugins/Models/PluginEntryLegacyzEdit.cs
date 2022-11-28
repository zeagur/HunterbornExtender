using DynamicData;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Noggog;
using System.Windows.Forms;

namespace HunterbornExtenderUI;

public class PluginEntryLegacyzEdit
{
    public string type { get; set; } = "";
    public string name { get; set; } = "";
    public string properName { get; set; } = "";
    public string sortName { get; set; } = "";
    public string animalSwitch { get; set; } = "";
    public string carcassMessageBox { get; set; } = "";
    public string carcassSize { get; set; } = "1";
    public string carcassWeight { get; set; } = "1";
    public string carcassValue { get; set; } = "1";
    public string[] peltCount { get; set; } = new string[4] { "1", "1", "1", "1" };
    public string[] furPlateCount { get; set; } = new string[4] { "1", "1", "1", "1" };
    public string meat { get; set; } = "";
    public List<Dictionary<string, int>> mats = new();
    public List<string> negativeTreasure { get; set; } = new();
    public string sharedDeathItems { get; set; } = "";
    public string bloodType { get; set; } = "";
    public string venom { get; set; } = "";
    public string voice { get; set; } = "";

    public PluginEntry ToPluginEntry(ILinkCache linkCache)
    {
        PluginEntry plugin = new();
        plugin.Type = EntryTypeConverter.EntryStringToEnum(type);
        plugin.Name = name;
        plugin.ProperName = properName;
        plugin.SortName = sortName;
        if (linkCache.TryResolve<IGlobalGetter>(animalSwitch, out var animalSwitchGetter))
        {
            plugin.AnimalSwitch = animalSwitchGetter.FormKey;
        }
        if (linkCache.TryResolve<IMessageGetter>(carcassMessageBox, out var carcassMessageBoxGetter))
        {
            plugin.CarcassMessageBox = carcassMessageBoxGetter.FormKey;
        }
        if (int.TryParse(carcassSize, out var size))
        {
            plugin.CarcassSize = Convert.ToInt32(size);
        }
        else
        {
            plugin.CarcassSize = 1;
        }
        if (int.TryParse(carcassWeight, out var weight))
        {
            plugin.CarcassWeight = Convert.ToInt32(weight);
        }
        else
        {
            plugin.CarcassWeight = 1;
        }
        if (int.TryParse(carcassValue, out var value))
        {
            plugin.CarcassValue = Convert.ToInt32(value);
        }
        else
        {
            plugin.CarcassValue = 1;
        }
        plugin.PeltCount = peltCount;
        plugin.FurPlateCount = furPlateCount;
        if (linkCache.TryResolve<IIngestibleGetter>(meat, out var meatGetter))
        {
            plugin.Meat = meatGetter.FormKey;
        }

        foreach (var mat in mats)
        {
            Dictionary<FormKey, int> dict = new();
            foreach (var keyStr in mat.Keys)
            {
                if (linkCache.TryResolve<IIngredientGetter>(meat, out var matKeyGetter))
                {
                    dict.Add(matKeyGetter.FormKey, mat[keyStr]);
                }
            }
            if (dict.Keys.Any())
            {
                plugin.Mats.Add(dict);
            }
        }

        foreach (var nt in negativeTreasure)
        {
            if (linkCache.TryResolve<IIngestibleGetter>(nt, out var ntGetter))
            {
                plugin.NegativeTreasure.Add(ntGetter.FormKey);
            }
        }

        if (linkCache.TryResolve<ILeveledItem>(sharedDeathItems, out var sharedDeathItemGetter))
        {
            plugin.SharedDeathItems = sharedDeathItemGetter.FormKey;
        }

        if (linkCache.TryResolve<IIngestibleGetter>(bloodType, out var bloodTypeGetter))
        {
            plugin.BloodType = bloodTypeGetter.FormKey;
        }

        if (linkCache.TryResolve<IIngestibleGetter>(venom, out var venomGetter))
        {
            plugin.Venom = venomGetter.FormKey;
        }

        if (linkCache.TryResolve<IVoiceTypeGetter>(voice, out var voiceGetter))
        {
            plugin.Voice = voiceGetter.FormKey;
        }

        /* Debugging formkey loading
        if (FormKey.TryFactory("00F8AE:Dawnguard.esm", out var gargoyleVoiceFormKey))
        {
            bool resolvedAsVoiceTypeGetter = false;
            bool resolvedAsVoiceType = false;
            bool resolvedAsGeneric = false;
            
            if (linkCache.TryResolve<IVoiceTypeGetter>(gargoyleVoiceFormKey, out var gargoyleVoice_VTgetter))
            {
                resolvedAsVoiceTypeGetter = true;
            }

            if (linkCache.TryResolve<IVoiceType>(gargoyleVoiceFormKey, out var gargoyleVoice_VT))
            {
                resolvedAsVoiceType = true;
            }

            if (linkCache.TryResolve(gargoyleVoiceFormKey, out var gargoyleVoice_Generic))
            {
                resolvedAsGeneric = true;
            }
        }*/

        return plugin;
    }
}