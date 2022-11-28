using Mutagen.Bethesda.Oblivion;
using SynthEBD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HunterbornExtenderUI
{
    public class PatcherSettings
    {
        public List<DeathItemSelection> DeathItems { get; set; } = new();
        public List<PluginEntry> CreatureData { get; set; } = new();

        public static PatcherSettings DumpToSettings(VM_DeathItemSelectionList deathItemsVM, VM_PluginList pluginsVM)
        {
            PatcherSettings settings = new();
            settings.DeathItems = deathItemsVM.DeathItems.Select(x => x.DumpToModel()).ToList();
            foreach (var plugin in pluginsVM.Plugins)
            {
                foreach (var entry in plugin.Entries)
                {
                    settings.CreatureData.Add(entry.DumpToModel());
                }
            }
            return settings;
        }

        public void SaveToDisk(string folderPath)
        {
            var path = System.IO.Path.Combine(folderPath, "settings.json");
            JSONhandler<PatcherSettings>.SaveJSONFile(this, path);
        }

        public static PatcherSettings LoadFromDisk(string folderPath)
        {
            var path = System.IO.Path.Combine(folderPath, "settings.json");
            if (System.IO.File.Exists(path))
            {
                return JSONhandler<PatcherSettings>.LoadJSONFile(path) ?? new PatcherSettings();
            }
            return new PatcherSettings();
        }
    }
}
