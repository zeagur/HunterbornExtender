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
    private DataState _dataState;
    private readonly VMLoader_Plugins _vmPluginLoader;

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
        DataState dataState,
        VM_WelcomePage welcomePage,
        VM_PluginEditorPage pluginEditorPage,
        VMLoader_Plugins vmPluginLoader,
        VM_DeathItemAssignmentPage deathItemMenu)
    {
        _pluginLoader = pluginLoader;
        _dataState = dataState;
        _vmPluginLoader = vmPluginLoader;

        WelcomePage = welcomePage;
        PluginEditorPage = pluginEditorPage;
        DeathItemMenu = deathItemMenu;

        Init();

        //DisplayedSubView = WelcomePage;
        DisplayedSubView = PluginEditorPage;

        ClickDeathItemAssignment = ReactiveCommand.Create(
            () => DisplayedSubView = DeathItemMenu);
            
        ClickPluginsMenu = ReactiveCommand.Create(
            () => DisplayedSubView = PluginEditorPage);

        Test = ReactiveCommand.Create(
            () => MessageBox.Show("Test"));
    }
    
    public void Init()
    {
        _dataState.Plugins.SetTo(_pluginLoader.LoadPlugins());
        PluginEditorPage.Plugins = _vmPluginLoader.GetPluginVMs(_dataState.Plugins.Items);
    }
}