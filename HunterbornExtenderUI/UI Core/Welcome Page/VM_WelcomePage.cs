using Noggog.WPF;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using HunterbornExtender;
using System.Reflection;
using System.Windows.Input;

namespace HunterbornExtenderUI;

public class VM_WelcomePage : ViewModel
{
    private readonly ObservableAsPropertyHelper<int> _pluginCount;
    public int PluginCount => _pluginCount.Value;
    [Reactive]
    public string SettingsDir { get; set; } = String.Empty;
    public ICommand SetSettingsDir { get; }

    public VM_WelcomePage(VM_PluginList pluginList)
    {
        _pluginCount = pluginList.WhenAnyValue(x => x.Plugins.Count)
            .ToProperty(this, nameof(PluginCount));

        //Temporary directory setting code until environment creation is done via Synthesis
        string exeLocation = string.Empty;
        var assembly = Assembly.GetEntryAssembly();
        if (assembly != null && assembly.Location != null)
        {
            exeLocation = System.IO.Path.GetDirectoryName(assembly.Location) ?? string.Empty;
        }
        var saveInfoPath = System.IO.Path.Combine(exeLocation, "SavePath.json");
        if (System.IO.File.Exists(saveInfoPath))
        {
            var saveInfo = JSONhandler<string>.LoadJSONFile(saveInfoPath);
            if (saveInfo != null)
            {
                SettingsDir = saveInfo;
            }
        }

        this.WhenAnyValue(x => x.SettingsDir).Subscribe(_ =>
        {
            JSONhandler<string>.SaveJSONFile(SettingsDir, saveInfoPath);
        });

        SetSettingsDir = ReactiveCommand.Create(() => 
            {
                System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    SettingsDir = dialog.SelectedPath;
                }
            });
        // end directory setting
    }

    public void ForceSettingsDir(string settingsDir)
    {
        SettingsDir = settingsDir;
    }
}