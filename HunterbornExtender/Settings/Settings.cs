namespace HunterbornExtender.Settings;

using Mutagen.Bethesda.Synthesis.States.DI;
using HunterbornExtenderUI;


public class Settings
{
    public Plugin[] Plugins { get; set; } = new Plugin[0];

    public DeathItemSelection[] DeathItemSelections { get; set; } = new DeathItemSelection[0];


}