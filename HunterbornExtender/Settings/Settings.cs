namespace HunterbornExtender.Settings;

sealed public class Settings
{
    public List<PluginEntry> CreatureData { get; set; } = new();

    public DeathItemSelection[] DeathItemSelections { get; set; } = new DeathItemSelection[0];

    public bool DebuggingMode { get; set; } = true;

}