using SynthEBD;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HunterbornExtender.Settings;

namespace HunterbornExtenderUI
{
    public class DeathItemSettingsLoader
    {
        private StateProvider _state;
        public DeathItemSettingsLoader(StateProvider state, EDIDtoForm edidToForm)
        {
            _state = state;
        }
        public HashSet<DeathItemSelection> LoadDeathItemSettings()
        {
            HashSet<DeathItemSelection> deathItems = new();
            var deathItemsPath = Path.Combine(_state.ExtraSettingsDataPath, "DeathItems.json");
            if (File.Exists(deathItemsPath))
            {
                deathItems = JSONhandler<HashSet<DeathItemSelection>>.LoadJSONFile(deathItemsPath) ?? new();
            }
            return deathItems;
        }
    }
}
