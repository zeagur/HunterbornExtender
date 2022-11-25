using Mutagen.Bethesda.Synthesis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HunterbornExtenderUI
{
    public class VMLoader_Plugins
    {
        private StateProvider _state;
        public VMLoader_Plugins(StateProvider state)
        {
            _state = state;
        }   

        public ObservableCollection<VM_Plugin> GetPluginVMs(HashSet<Plugin> models)
        {
            ObservableCollection<VM_Plugin> result = new();
            foreach (var model in models)
            {
                var viewModel = new VM_Plugin(_state);
                viewModel.LoadFromModel(model);
                result.Add(viewModel);
            }
            return result;
        }
    }
}
