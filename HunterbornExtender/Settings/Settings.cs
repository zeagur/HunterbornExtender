namespace HunterbornExtender.Settings;

sealed public class Settings
{
    public List<PluginEntry> CreatureData { get; set; } = new();

    public DeathItemSelection[] DeathItemSelections { get; set; } = Array.Empty<DeathItemSelection>();

    public bool DebuggingMode { get; set; } = true;

    public bool ReuseSelections { get; set; } = true;

}