using System.Windows;
using System.Windows.Input;
using Noggog;
using Noggog.WPF;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace HunterbornExtenderUI;

public class MainWindowVM : ViewModel
{
    private PluginLoader _pluginLoader;
    private readonly VMLoader_Plugins _vmPluginLoader;
    private readonly VM_PluginList _pluginList;

    [Reactive]
    public object DisplayedSubView { get; set; }
    public VM_WelcomePage WelcomePage { get; }
    public VM_DeathItemAssignmentPage DeathItemMenu { get; }
    public VM_PluginEditorPage PluginEditorPage { get; }

    public ICommand ClickDeathItemAssignment { get; }
    public ICommand ClickPluginsMenu { get; }
    public ICommand Test { get; }
        
    public MainWindowVM(
        PluginLoader pluginLoader,
        VM_WelcomePage welcomePage,
        VM_PluginEditorPage pluginEditorPage,
        VMLoader_Plugins vmPluginLoader,
        VM_PluginList pluginList,
        VM_DeathItemAssignmentPage deathItemMenu)
    {
        _pluginLoader = pluginLoader;
        _vmPluginLoader = vmPluginLoader;
        _pluginList = pluginList;

        WelcomePage = welcomePage;
        PluginEditorPage = pluginEditorPage;
        DeathItemMenu = deathItemMenu;

        Init();

        DisplayedSubView = WelcomePage;

        ClickDeathItemAssignment = ReactiveCommand.Create(
            () => DisplayedSubView = DeathItemMenu);

        ClickPluginsMenu = ReactiveCommand.Create(
            () => DisplayedSubView = PluginEditorPage);

        Test = ReactiveCommand.Create(
            () => MessageBox.Show("Test"));
    }
    
    public void Init()
    {
        _pluginList.Plugins.SetTo(
            _vmPluginLoader.GetPluginVMs(
                _pluginLoader.LoadPlugins()));
    }
}