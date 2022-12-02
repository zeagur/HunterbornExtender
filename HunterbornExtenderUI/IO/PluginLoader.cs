using SynthEBD;
using System.IO;
using HunterbornExtender.Settings;

namespace HunterbornExtenderUI;

public class PluginLoader
{
    private StateProvider _state;
    private EDIDtoForm _edidToForm;
    public PluginLoader(StateProvider state, EDIDtoForm edidToForm)
    {
        _state = state;
        _edidToForm = edidToForm;
    }

    public HashSet<Plugin> LoadPlugins()
    {
        HashSet<Plugin> plugins = new ();
        var pluginsPath = Path.Combine(_state.ExtraSettingsDataPath, "Plugins");
        if (Directory.Exists(pluginsPath))
        {
            foreach (var path in Directory.EnumerateFiles(pluginsPath))
            {
                if (path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    var loadedPlugin = new Plugin();
                    var entries = JSONhandler<List<PluginEntry>>.LoadJSONFile(path);
                    if (entries == null)
                    {
                        loadedPlugin = ConvertPluginFromzEdit(path);
                    }

                    if (loadedPlugin != null)
                    {
                        loadedPlugin.FilePath = path;
                        plugins.Add(loadedPlugin);
                    }
                    else
                    {
                        //log
                    }
                }
            }
        }
        else
        {
            //log
        }
        return plugins;
    }

    public Plugin? ConvertPluginFromzEdit(string path)
    {
        try
        {
            var entries = JSONhandler<List<PluginEntryLegacyzEdit>>.LoadJSONFile(path);
            if (entries != null)
            {
                PluginLegacyzEdit legacyPlugin = new();
                legacyPlugin.Entries = entries;
                Plugin converted = legacyPlugin.ToPluginEntry(_state.LinkCache);
                converted.FilePath = path;
                return converted;
            }
            else
            {
                return null;
            }
        }
        catch
        {
            return null;
        }
    }
}