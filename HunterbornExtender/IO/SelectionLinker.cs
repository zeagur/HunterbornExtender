using HunterbornExtender.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HunterbornExtender.IO
{
    public class SelectionLinker
    {
        public static void LinkDeathItemSelections(IEnumerable<DeathItemSelection> selections, IEnumerable<PluginEntry> entries)
        {
            foreach (var selection in selections)
            {
                selection.Selection = entries.Where(x => x.SortName == selection.CreatureEntryName).FirstOrDefault();
            }    
        }
    }
}
