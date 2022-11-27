using Mutagen.Bethesda.Plugins.Cache;

namespace HunterbornExtenderUI;

public class PluginLegacyzEdit
{
    public List<PluginEntryLegacyzEdit> Entries { get; set; } = new();

    public Plugin ToPluginEntry(ILinkCache linkCache)
    {
        Plugin plugin = new();
        foreach (var entry in Entries)
        {
            var convertedEntry = entry.ToPluginEntry(linkCache);
            if (convertedEntry != null)
            {
                plugin.Entries.Add(convertedEntry);
            }
            else
            {
                // log
            }
        }
        return plugin;
    }
}