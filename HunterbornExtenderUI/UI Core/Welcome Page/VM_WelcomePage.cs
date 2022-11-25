using Noggog.WPF;
using ReactiveUI;

namespace HunterbornExtenderUI;

public class VM_WelcomePage : ViewModel
{
    private readonly ObservableAsPropertyHelper<int> _pluginCount;
    public int PluginCount => _pluginCount.Value;
    
    private DataState _dataState { get; set; }

    public VM_WelcomePage(DataState dataState)
    {
        _dataState = dataState;
        _pluginCount = _dataState.Plugins.CountChanged
            .ToProperty(this, nameof(PluginCount));
    }
}