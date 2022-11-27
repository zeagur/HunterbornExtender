using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HunterbornExtenderUI
{
    public class VM_DeathItemSelectionList
    {
        public ObservableCollection<VM_DeathItemSelection> DeathItems { get; } = new();
    }
}
