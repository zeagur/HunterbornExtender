using System.Windows;
using System.Windows.Input;
using HunterbornExtenderUI.App_Core.Death_Item_Selection;
using Noggog;
using Noggog.WPF;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using HunterbornExtender.Settings;

namespace HunterbornExtenderUI;

public class MainWindowVM : ViewModel
{
    private PluginLoader _pluginLoader;
    private readonly VMLoader_Plugins _vmPluginLoader;
    private readonly VM_PluginList _pluginList;

    private DeathItemSettingsLoader _deathItemLoader;
    private readonly VMLoader_DeathItems _vmDeathItemLoader;
    private readonly VM_DeathItemSelectionList _deathItemSelectionList;

    private Settings _patcherSettings;

    [Reactive]
    public object DisplayedSubView { get; set; }
    public VM_WelcomePage WelcomePage { get; }
    public VM_DeathItemAssignmentPage DeathItemMenu { get; }
    public VM_PluginEditorPage PluginEditorPage { get; }

    public ICommand ClickDeathItemAssignment { get; }
    public ICommand ClickPluginsMenu { get; }
    public ICommand SaveSettings { get; }

    public MainWindowVM(
        PluginLoader pluginLoader,
        DeathItemSettingsLoader deathItemSettingsLoader,
        VM_WelcomePage welcomePage,
        VM_PluginEditorPage pluginEditorPage,
        VMLoader_Plugins vmPluginLoader,
        VMLoader_DeathItems deathItemLoader,
        VM_PluginList pluginList,
        VM_DeathItemSelectionList deathItemSelectionList,
        VM_DeathItemAssignmentPage deathItemMenu)
    {
        _pluginLoader = pluginLoader;
        _deathItemLoader = deathItemSettingsLoader;

        _pluginList = pluginList;
        _deathItemSelectionList = deathItemSelectionList;

        _vmPluginLoader = vmPluginLoader;
        _vmDeathItemLoader = deathItemLoader;

        WelcomePage = welcomePage;
        PluginEditorPage = pluginEditorPage;
        DeathItemMenu = deathItemMenu;

        _patcherSettings = PatcherSettingsIO.LoadFromDisk(WelcomePage.SettingsDir); // change this to state.ExtraDataSettingsFolder when it becomes exposed

        Init();

        DisplayedSubView = WelcomePage;

        ClickDeathItemAssignment = ReactiveCommand.Create(
            () => DisplayedSubView = DeathItemMenu);

        ClickPluginsMenu = ReactiveCommand.Create(
            () => DisplayedSubView = PluginEditorPage);

        SaveSettings = ReactiveCommand.Create(() =>
        {
            var settings = PatcherSettingsIO.DumpToSettings(deathItemSelectionList, pluginList);
            if (WelcomePage.SettingsDir != string.Empty && System.IO.Directory.Exists(WelcomePage.SettingsDir))
            {
                PatcherSettingsIO.SaveToDisk(WelcomePage.SettingsDir, PatcherSettingsIO.DumpToSettings(deathItemSelectionList, pluginList));
                MessageBox.Show("Saved to " + System.IO.Path.Combine(WelcomePage.SettingsDir, "settings.json"));
            }
            else
            {
                MessageBox.Show("Could not find directory " + WelcomePage.SettingsDir);
            }
        });
    }
    
    public void Init()
    {
        _pluginList.Plugins.SetTo(
            _vmPluginLoader.GetPluginVMs(
                _pluginLoader.LoadPlugins()));

        /*
        _deathItemSelectionList.DeathItems.SetTo(
            _vmDeathItemLoader.GetDeathItemVMs(
                _deathItemLoader.LoadDeathItemSettings()));
        */
        DeathItemMenu.Initialize(_patcherSettings.DeathItemSelections);
    }
}