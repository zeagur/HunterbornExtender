using HunterbornExtender.Settings;
using SynthEBD;

namespace HunterbornExtenderUI
{
    public class PatcherSettingsIO
    {

        public static Settings DumpToSettings(VM_DeathItemSelectionList deathItemsVM, VM_PluginList pluginsVM)
        {
            Settings settings = new();
            settings.DeathItemSelections = deathItemsVM.DeathItems.Select(x => x.DumpToModel()).ToArray();
            foreach (var plugin in pluginsVM.Plugins)
            {
                foreach (var entry in plugin.Entries)
                {
                    settings.CreatureData.Add(entry.DumpToModel());
                }
            }
            return settings;
        }

        public static void SaveToDisk(string folderPath, Settings settings)
        {
            var path = System.IO.Path.Combine(folderPath, "settings.json");
            JSONhandler<Settings>.SaveJSONFile(settings, path);
        }

        public static Settings LoadFromDisk(string folderPath)
        {
            var path = System.IO.Path.Combine(folderPath, "settings.json");
            if (System.IO.File.Exists(path))
            {
                return JSONhandler<Settings>.LoadJSONFile(path) ?? new Settings();
            }
            return new Settings();
        }
    }
}
