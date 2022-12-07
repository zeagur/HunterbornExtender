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
    /// <summary>
    /// Maps DeathItems (the leveleditems in the INAM field of NPCs) to instances of PluginEntry.
    /// The patcher uses the PluginEntry to create the forms and array property entries for creatures with 
    /// that deathitem.
    /// 
    /// Only the DeathItem and the PluginEntry name (CreatureEntryName) are serialized.
    /// 
    /// CreatureEntryName is used to restore the Selection field after deserialization.
    /// 
    /// AssignedNPCs is only used during the heuristic matching set and in the UI. 
    /// It is NOT needed during patching.
    /// 
    /// </summary>
    sealed public class DeathItemSelection
    {
        public FormKey DeathItem { get; set; }
        public string CreatureEntryName { get; set; } = String.Empty;
        [JsonIgnore]
        public PluginEntry Selection { get; set; } = PluginEntry.SKIP;
        [JsonIgnore]
        public HashSet<INpcGetter> AssignedNPCs { get; set; } = new(); // does the patcher actually need to know this or does it solely concern the UI? Leaving it for now because Program.cs appears to reference it.
    }
}
