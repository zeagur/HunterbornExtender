using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HunterbornExtender.Settings;

namespace HunterbornExtenderUI.App_Core.Death_Item_Selection
{
    public class VMLoader_DeathItems
    {
        private readonly Func<VM_DeathItemSelection> _vmDeathItemFactory;
        public VMLoader_DeathItems(Func<VM_DeathItemSelection> vmDeathItemFactory)
        {
            _vmDeathItemFactory = vmDeathItemFactory;
        }

        public IEnumerable<VM_DeathItemSelection> GetDeathItemVMs(IEnumerable<DeathItemSelection> models)
        {
            foreach (var model in models)
            {
                var viewModel = _vmDeathItemFactory();
                viewModel.LoadFromModel(model);
                yield return viewModel;
            }
        }
    }

}
