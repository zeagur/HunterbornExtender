using System.Collections.ObjectModel;
using System.Windows.Input;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace HunterbornExtenderUI;

public class VM_PluginEditorPage
{
    [Reactive]
    public VM_Plugin? DisplayedPlugin { get; set; } = null;
    public VM_PluginList PluginList { get; set; }
    public ICommand AddPlugin { get; }
    public ICommand DeletePlugin { get; }

    public VM_PluginEditorPage(
        VM_PluginList pluginList,
        Func<VM_Plugin> pluginFactory)
    {
        PluginList = pluginList;

        AddPlugin = ReactiveCommand.Create(
            () => pluginList.Plugins.Add(pluginFactory()));

        DeletePlugin = ReactiveCommand.Create<VM_Plugin>(
            x => {
                if (System.IO.File.Exists(x.FilePath))
                {
                    System.IO.File.Delete(x.FilePath);
                }
                pluginList.Plugins.Remove(x);
                });
    }
}