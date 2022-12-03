using DynamicData;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using HunterbornExtender.Settings;
using System.Windows.Forms;

namespace HunterbornExtenderUI;

public class PluginEntryLegacyzEdit // do not rename these properties - they correspond to the original zEdit patcher format
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
        AddonPluginEntry entry = new(EntryTypeConverter.EntryStringToEnum(type), name);
        entry.ProperName = properName;
        entry.SortName = sortName;
        if (linkCache.TryResolve<IGlobalGetter>(animalSwitch, out var ToggleGetter))
        {
            entry.Toggle = ToggleGetter.ToLinkGetter();
        }
        if (linkCache.TryResolve<IMessageGetter>(carcassMessageBox, out var carcassMessageBoxGetter))
        {
            entry.CarcassMessageBox = carcassMessageBoxGetter.ToLinkGetter();
        }
        if (int.TryParse(carcassSize, out var size))
        {
            entry.CarcassSize = Convert.ToInt32(size);
        }
        else
        {
            entry.CarcassSize = 1;
        }
        if (int.TryParse(carcassWeight, out var weight))
        {
            entry.CarcassWeight = Convert.ToInt32(weight);
        }
        else
        {
            entry.CarcassWeight = 1;
        }
        if (int.TryParse(carcassValue, out var value))
        {
            entry.CarcassValue = Convert.ToInt32(value);
        }
        else
        {
            entry.CarcassValue = 1;
        }
        entry.PeltCount = peltCount.Select(x => Convert.ToInt32(x)).ToArray();
        entry.FurPlateCount = furPlateCount.Select(x => Convert.ToInt32(x)).ToArray();
        if (linkCache.TryResolve<IIngestibleGetter>(meat, out var meatGetter))
        {
            entry.Meat = meatGetter.ToLinkGetter();
        }

        foreach (var mat in mats)
        {
            Dictionary<IFormLinkGetter<IItemGetter>, int> dict = new();
            foreach (var keyStr in mat.Keys)
            {
                if (linkCache.TryResolve<IItemGetter>(keyStr, out var matKeyGetter))
                {
                    dict.Add(matKeyGetter.ToLinkGetter(), mat[keyStr]);
                }
            }
            if (dict.Keys.Any())
            {
                entry.Materials.Add(new MaterialLevel() { Materials = dict});
            }
        }

        foreach (var nt in negativeTreasure)
        {
            if (linkCache.TryResolve<IIngestibleGetter>(nt, out var ntGetter))
            {
                entry.Discard.Add(ntGetter.FormKey);
            }
        }

        if (linkCache.TryResolve<IFormListGetter>(sharedDeathItems, out var sharedDeathItemGetter))
        {
            entry.SharedDeathItems = sharedDeathItemGetter.ToLinkGetter();
        }

        if (linkCache.TryResolve<IItemGetter>(bloodType, out var bloodTypeGetter))
        {
            entry.BloodType = bloodTypeGetter.ToLinkGetter();
        }

        if (linkCache.TryResolve<IItemGetter>(venom, out var venomGetter))
        {
            entry.Venom = venomGetter.ToLinkGetter();
        }

        if (linkCache.TryResolve<IVoiceTypeGetter>(voice, out var voiceGetter))
        {
            entry.Voice = voiceGetter.ToLinkGetter();
        }

        return entry;
    }
}