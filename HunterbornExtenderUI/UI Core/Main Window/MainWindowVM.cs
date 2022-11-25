using System.Windows;
using System.Windows.Input;
using Noggog.WPF;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace HunterbornExtenderUI;

public class MainWindowVM : ViewModel
{
    private StateProvider _stateProvider;
    private PluginLoader _pluginLoader;
    private DataState _dataState;

    [Reactive]
    public object DisplayedSubView { get; set; }
    public VM_WelcomePage WelcomePage { get; set; }
    public VM_DeathItemAssignmentPage DeathItemMenu { get; set; }
    public VM_PluginEditorPage PluginEditorPage { get; set; }

    public ICommand ClickDeathItemAssignment { get; }
    public ICommand ClickPluginsMenu { get; }
    public ICommand Test { get; }
        
    public MainWindowVM(StateProvider stateProvider, PluginLoader pluginLoader, DataState dataState)
    {
        _stateProvider = stateProvider;
        _pluginLoader = pluginLoader;
        _dataState = dataState;

        WelcomePage = new(_dataState);
        PluginEditorPage = new(_stateProvider);
        DeathItemMenu = new(_stateProvider, _dataState);

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
        _dataState.Plugins = _pluginLoader.LoadPlugins();
        PluginEditorPage.Plugins = new VMLoader_Plugins(_stateProvider).GetPluginVMs(_dataState.Plugins);
    }
}