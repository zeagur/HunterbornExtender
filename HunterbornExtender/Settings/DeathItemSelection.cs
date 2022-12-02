using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Newtonsoft.Json;

namespace HunterbornExtender.Settings
{
    public class DeathItemSelection
    {
        public FormKey DeathItemList { get; set; }
        public string CreatureEntryName { get; set; } = String.Empty;
        [JsonIgnore]
        public PluginEntry? Selection { get; set; } = null;
        [JsonIgnore]
        public HashSet<INpcGetter> AssignedNPCs { get; set; } = new(); // does the patcher actually need to know this or does it solely concern the UI? Leaving it for now because Program.cs appears to reference it.
    }
}
