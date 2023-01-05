using HunterbornExtender;
using System.IO;
using HunterbornExtender.Settings;

namespace HunterbornExtenderUI;

public class PluginLoader
{
    private IStateProvider _state;
    private EDIDtoForm _edidToForm;
    public PluginLoader(IStateProvider state, EDIDtoForm edidToForm)
    {
        _state = state;
        _edidToForm = edidToForm;
    }

    public HashSet<Plugin> LoadPlugins()
    {
        HashSet<Plugin> plugins = new ();

        var userPluginsPath = Path.Combine(_state.ExtraSettingsDataPath, "Plugins");
        var defaultPluginsPath = Path.Combine(_state.InternalDataPath, "Plugins");

        LoadPluginsFromDirectory(userPluginsPath, plugins, false);
        LoadPluginsFromDirectory(defaultPluginsPath, plugins, true);

        return plugins;
    }

    public void LoadPluginsFromDirectory(string directoryPath, HashSet<Plugin> plugins, bool fromInternalData)
    {
        if (Directory.Exists(directoryPath))
        {
            foreach (var path in Directory.EnumerateFiles(directoryPath))
            {
                if (path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    if (fromInternalData && plugins.Where(x => Path.GetFileNameWithoutExtension(x.FilePath) == Path.GetFileNameWithoutExtension(path)).Any()) { continue; } // do not overwrite user's customized plugins with the originals packaged with the patcher

                    var loadedPlugin = JSONhandler<Plugin>.LoadJSONFile(path);
                    if (loadedPlugin == null)
                    {
                        loadedPlugin = ConvertPluginFromzEdit(path);
                    }
                    else
                    {
                        loadedPlugin.FilePath = path;
                    }

                    if (loadedPlugin != null)
                    {
                        if (fromInternalData)
                        {
                            loadedPlugin.FilePath = Path.Combine(_state.ExtraSettingsDataPath, "Plugins", Path.GetFileName(path)); // remap plugins from the default InternalDataPath to save in the user's ExtraSettingsDataPath
                        }
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