using HunterbornExtender;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HunterbornExtender.Settings;
using System.Windows;

namespace HunterbornExtenderUI
{
    public class DeathItemSettingsLoader
    {
        private IStateProvider _state;
        public DeathItemSettingsLoader(IStateProvider state, EDIDtoForm edidToForm)
        {
            _state = state;
        }
        public HashSet<DeathItemSelection> LoadDeathItemSettings()
        {
            HashSet<DeathItemSelection> deathItems = new();
            var settingsPath = Path.Combine(_state.ExtraSettingsDataPath, "Settings.json");
            if (File.Exists(settingsPath))
            {
                var settings = JSONhandler<Settings>.LoadJSONFile(settingsPath, out var exceptionStr);
                
                if (settings != null)
                {
                    deathItems = settings.DeathItemSelections.ToHashSet();
                }

                if (!string.IsNullOrEmpty(exceptionStr))
                {
                    MessageBox.Show("Could not read Settings.json: " + Environment.NewLine + exceptionStr);
                }
            }
            return deathItems;
        }
    }
}
