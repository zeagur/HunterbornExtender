using System.Collections.ObjectModel;
using Noggog.WPF;

namespace HunterbornExtenderUI;

public class VM_PluginList : ViewModel
{
    public ObservableCollection<VM_Plugin> Plugins { get; } = new();
}