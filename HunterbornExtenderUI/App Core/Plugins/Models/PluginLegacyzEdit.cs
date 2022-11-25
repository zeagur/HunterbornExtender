namespace HunterbornExtenderUI;

public class PluginLegacyzEdit
{
    public List<PluginEntryLegacyzEdit> Entries { get; set; } = new();

    public Plugin ToPluginEntry(EDIDtoForm edidConverter)
    {
        Plugin plugin = new();
        foreach (var entry in Entries)
        {
            var convertedEntry = entry.ToPluginEntry(edidConverter);
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