using System.Collections.ObjectModel;

namespace HunterbornExtenderUI;

public class VMLoader_Plugins
{
    private readonly Func<VM_Plugin> _vmPluginFactory;
    public VMLoader_Plugins(Func<VM_Plugin> vmPluginFactory)
    {
        _vmPluginFactory = vmPluginFactory;
    }   

    public IEnumerable<VM_Plugin> GetPluginVMs(IEnumerable<Plugin> models)
    {
        foreach (var model in models)
        {
            var viewModel = _vmPluginFactory();
            viewModel.LoadFromModel(model);
            yield return viewModel;
        }
    }
}