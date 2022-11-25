using System.Collections.ObjectModel;
using Noggog.WPF;
using ReactiveUI.Fody.Helpers;

namespace HunterbornExtenderUI;

public class VM_PluginList : ViewModel
{
    public ObservableCollection<VM_Plugin> Plugins { get; } = new();
}