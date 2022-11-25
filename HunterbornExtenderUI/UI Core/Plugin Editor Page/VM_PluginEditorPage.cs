using System.Collections.ObjectModel;
using System.Windows.Input;
using ReactiveUI;

namespace HunterbornExtenderUI;

public class VM_PluginEditorPage
{
    public ObservableCollection<VM_Plugin> Plugins { get; set; } = new();
    public VM_Plugin? DisplayedPlugin { get; set; } = null;

    public ICommand AddPlugin { get; }
    public ICommand DeletePlugin { get; }

    public VM_PluginEditorPage(Func<VM_Plugin> pluginFactory)
    {
        AddPlugin = ReactiveCommand.Create(
            () => Plugins.Add(pluginFactory()));

        DeletePlugin = ReactiveCommand.Create<VM_Plugin>(
            x => Plugins.Remove(x));
    }
}