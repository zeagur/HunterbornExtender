using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HunterbornExtenderUI
{
    public class DeathItemSelection
    {
        public FormKey DeathItemList { get; set; }
        public string CreatureEntryName { get; set; } = String.Empty;

        //public PluginEntry? Selection { get; set; } = null;

        [JsonIgnore]
        public HashSet<INpcGetter> AssignedNPCs { get; set; } = new();

    }
}
