using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HunterbornExtenderUI
{
    public class VM_PluginEditorPage
    {
        private StateProvider _stateProvider;
        public ObservableCollection<VM_Plugin> Plugins { get; set; } = new();
        public VM_Plugin? DisplayedPlugin { get; set; } = null;

        public RelayCommand AddPlugin { get; }
        public RelayCommand DeletePlugin { get; }

        public VM_PluginEditorPage(StateProvider stateProvider)
        {
            _stateProvider = stateProvider;

            AddPlugin = new RelayCommand(
                canExecute: _ => true,
                execute: _ => Plugins.Add(new VM_Plugin(_stateProvider))
            );

            DeletePlugin = new RelayCommand(
                canExecute: _ => true,
                execute: x => Plugins.Remove((VM_Plugin)x)
            );
        }
    }
}
