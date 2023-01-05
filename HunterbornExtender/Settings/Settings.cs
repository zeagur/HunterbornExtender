namespace HunterbornExtender.Settings;

sealed public class Settings
{
    public List<PluginEntry> PluginEntries { get; set; } = new();

    public DeathItemSelection[] DeathItemSelections { get; set; } = Array.Empty<DeathItemSelection>();

    public bool DebuggingMode { get; set; } = true;

    public bool ReuseSelections { get; set; } = true;

    public bool AdvancedTaxonomy { get; set; } = true;

    public bool QuickLootPatch { get; set; } = true;

}