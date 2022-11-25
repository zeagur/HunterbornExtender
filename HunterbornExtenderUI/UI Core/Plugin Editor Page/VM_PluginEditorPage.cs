using System.Collections.ObjectModel;
using System.Windows.Input;
using ReactiveUI;

namespace HunterbornExtenderUI;

public class VM_PluginEditorPage
{
    public VM_Plugin? DisplayedPlugin { get; set; } = null;

    public ICommand AddPlugin { get; }
    public ICommand DeletePlugin { get; }

    public VM_PluginEditorPage(
        VM_PluginList pluginList,
        Func<VM_Plugin> pluginFactory)
    {
        AddPlugin = ReactiveCommand.Create(
            () => pluginList.Plugins.Add(pluginFactory()));

        DeletePlugin = ReactiveCommand.Create<VM_Plugin>(
            x => pluginList.Plugins.Remove(x));
    }
}