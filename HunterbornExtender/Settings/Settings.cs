namespace HunterbornExtender.Settings;

sealed public class Settings
{
    public List<PluginEntry> CreatureData { get; set; } = new();

    public Dictionary<DeathItemSelection, PluginEntry?> DeathItemSelections { get; set; } = new();

    public bool DebuggingMode { get; set; } = true;

}