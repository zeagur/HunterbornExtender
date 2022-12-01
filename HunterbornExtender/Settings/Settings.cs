namespace HunterbornExtender.Settings;

using HunterbornExtenderUI;


sealed public class Settings
{
    public Plugin[] Plugins { get; set; } = new Plugin[0];

    public DeathItemSelection[] DeathItemSelections { get; set; } = new DeathItemSelection[0];

    public bool DebuggingMode { get; set; } = true;


}