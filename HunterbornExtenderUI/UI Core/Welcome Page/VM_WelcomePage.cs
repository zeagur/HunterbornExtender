using Noggog.WPF;
using ReactiveUI;

namespace HunterbornExtenderUI;

public class VM_WelcomePage : ViewModel
{
    private readonly ObservableAsPropertyHelper<int> _pluginCount;
    public int PluginCount => _pluginCount.Value;

    public VM_WelcomePage(VM_PluginList pluginList)
    {
        _pluginCount = pluginList.WhenAnyValue(x => x.Plugins.Count)
            .ToProperty(this, nameof(PluginCount));
    }
}