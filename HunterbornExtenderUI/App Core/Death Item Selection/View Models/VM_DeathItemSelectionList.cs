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
        public VM_DeathItemSelectionList()
        {
            CreatureAlphabetizer = new(DeathItems, x => x.CreatureEntryName, new(System.Windows.Media.Colors.MediumPurple));
            ItemAlphabetizer = new(DeathItems, x => x.DeathItem?.EditorID?.Replace("DeathItem", "", StringComparison.OrdinalIgnoreCase) ?? "", new(System.Windows.Media.Colors.MediumPurple));
        }
        public VM_Alphabetizer<VM_DeathItemSelection, string> CreatureAlphabetizer { get; set; }
        public VM_Alphabetizer<VM_DeathItemSelection, string> ItemAlphabetizer { get; set; }
        public ObservableCollection<VM_DeathItemSelection> DeathItems { get; } = new();
    }
}
