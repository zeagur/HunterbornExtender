using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins;

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

    public PluginEntry ToPluginEntry(EDIDtoForm edidConverter)
    {
        PluginEntry plugin = new();
        plugin.Type = EntryTypeConverter.EntryStringToEnum(type);
        plugin.Name = name;
        plugin.ProperName = properName;
        plugin.SortName = sortName;
        plugin.AnimalSwitch = edidConverter.GetFormFromEditorID<IGlobalGetter>(animalSwitch) ?? new FormKey();
        plugin.CarcassMessageBox = edidConverter.GetFormFromEditorID<IMessageGetter>(carcassMessageBox) ?? new FormKey();
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
        plugin.Meat = edidConverter.GetFormFromEditorID<IIngestibleGetter>(meat) ?? new FormKey();
            
        foreach (var mat in mats)
        {
            Dictionary<FormKey, int> dict = new();
            foreach (var keyStr in mat.Keys)
            {
                var keyForm = edidConverter.GetFormFromEditorID<IIngredientGetter>(keyStr);
                if (keyForm != null)
                {
                    dict.Add(keyForm.Value, mat[keyStr]);
                }
            }
            if (dict.Keys.Any())
            {
                plugin.Mats.Add(dict);
            }
        }

        foreach (var nt in negativeTreasure)
        {
            var ntForm = edidConverter.GetFormFromEditorID<IIngestibleGetter>(nt);
            if (ntForm != null)
            {
                plugin.NegativeTreasure.Add(ntForm.Value);
            }
        }

        plugin.SharedDeathItems = edidConverter.GetFormFromEditorID<ILeveledItem>(sharedDeathItems) ?? new FormKey();
        plugin.BloodType = edidConverter.GetFormFromEditorID<IIngestibleGetter>(bloodType) ?? new FormKey();
        plugin.Venom = edidConverter.GetFormFromEditorID<IIngestibleGetter>(venom) ?? new FormKey();
        plugin.Voice = edidConverter.GetFormFromEditorID<IVoiceTypeGetter>(voice) ?? new FormKey();
        return plugin;
    }
}