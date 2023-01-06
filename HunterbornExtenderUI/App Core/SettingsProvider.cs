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

            var settings = PatcherSettingsIO.LoadFromDisk(_welcomePage.SettingsDir);
            if (settings != null)
            {
                PatcherSettings = settings ?? new();
            }
        }
        public Settings PatcherSettings { get; set; } = new();
    }
}
