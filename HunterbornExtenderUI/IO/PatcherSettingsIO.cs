using HunterbornExtender.Settings;
using HunterbornExtender;
using System.IO;
using System.Windows;

namespace HunterbornExtenderUI
{
    public class PatcherSettingsIO
    {
        private readonly SettingsProvider _settingsProvider;
        public PatcherSettingsIO(SettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
        }

        public void DumpToSettings(VM_DeathItemSelectionList deathItemsVM, VM_PluginList pluginsVM)
        {
            _settingsProvider.PatcherSettings.DeathItemSelections = deathItemsVM.DeathItems.Select(x => x.DumpToModel()).ToArray();
            foreach (var plugin in pluginsVM.Plugins.Where(x => x.IsVisible))
            {
                foreach (var entry in plugin.Entries.Where(x => x.IsVisible))
                {
                    _settingsProvider.PatcherSettings.PluginEntries.Add(entry.DumpToModel());
                }
            }
        }

        public void SaveToDisk(string folderPath)
        {
            var path = System.IO.Path.Combine(folderPath, "settings.json");
            Directory.CreateDirectory(folderPath);
            JSONhandler<Settings>.SaveJSONFile(_settingsProvider.PatcherSettings, path);
        }

        public static Settings LoadFromDisk(string folderPath)
        {
            var path = System.IO.Path.Combine(folderPath, "settings.json");
            if (System.IO.File.Exists(path))
            {
                var settings =  JSONhandler<Settings>.LoadJSONFile(path, out string exceptionStr) ?? new Settings();
                if (!string.IsNullOrEmpty(exceptionStr))
                {
                    MessageBox.Show("Could not read Settings.json: " + Environment.NewLine + exceptionStr);
                }
                else
                {
                    /* For debugging
                    MessageBox.Show("Successfully loaded settings from " + path + Environment.NewLine + 
                        "Plugin entries: " + (settings?.PluginEntries.Count ?? 0) + Environment.NewLine + 
                        "Death Item Selections: " + settings?.DeathItemSelections.Length.ToString());
                    */
                    return settings ?? new();
                }
            }
            return new Settings();
        }
    }
}
