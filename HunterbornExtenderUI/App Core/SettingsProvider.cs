using System.IO;
using System.Windows;
using HunterbornExtender;
using HunterbornExtender.Settings;

namespace HunterbornExtenderUI
{
    public class SettingsProvider
    {
        private readonly VM_WelcomePage _welcomePage;
        public SettingsProvider(VM_WelcomePage welcomePage)
        {
            _welcomePage = welcomePage;

            var settingsPath = Path.Combine(_welcomePage.SettingsDir, "Settings.json");
            if (File.Exists(settingsPath))
            {
                var settings = JSONhandler<Settings>.LoadJSONFile(settingsPath, out var exceptionStr);

                if (!string.IsNullOrEmpty(exceptionStr))
                {
                    MessageBox.Show("Could not read Settings.json: " + Environment.NewLine + exceptionStr);
                }
                else
                {
                    MessageBox.Show("Successfully loaded settings. Plugin entries: " + settings?.Plugins.Count ?? "0");
                }

                PatcherSettings = settings ?? new();
            }
        }
        public Settings PatcherSettings { get; set; } = new();
    }
}
