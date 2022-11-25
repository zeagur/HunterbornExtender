using Noggog.WPF;

namespace HunterbornExtenderUI;

public class VM_WelcomePage : ViewModel
{
    public int PluginCount { get; set; } = 0;
    private DataState _dataState { get; set; }

    public VM_WelcomePage(DataState dataState)
    {
        _dataState = dataState;
        PluginCount = _dataState.Plugins.Count();
    }
}