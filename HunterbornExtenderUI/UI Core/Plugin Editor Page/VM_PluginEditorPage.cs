using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;

namespace HunterbornExtenderUI
{
    public class VM_PluginEditorPage
    {
        private StateProvider _stateProvider;
        public ObservableCollection<VM_Plugin> Plugins { get; set; } = new();
        public VM_Plugin? DisplayedPlugin { get; set; } = null;

        public ICommand AddPlugin { get; }
        public ICommand DeletePlugin { get; }

        public VM_PluginEditorPage(StateProvider stateProvider)
        {
            _stateProvider = stateProvider;

            AddPlugin = ReactiveCommand.Create(
                () => Plugins.Add(new VM_Plugin(_stateProvider)));

            DeletePlugin = ReactiveCommand.Create<VM_Plugin>(
                x => Plugins.Remove(x));
        }
    }
}
