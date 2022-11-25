using Mutagen.Bethesda.Plugins;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HunterbornExtenderUI
{
    public class PluginEntry
    {
        public EntryType Type { get; set; } = EntryType.Monster;
        public string Name { get; set; } = "";
        public string ProperName { get; set; } = "";
        public string SortName { get; set; } = "";
        public FormKey AnimalSwitch { get; set; } = new();
        public FormKey CarcassMessageBox { get; set; } = new();
        public int CarcassSize { get; set; } = 1;
        public int CarcassWeight { get; set; } = 1;
        public int CarcassValue { get; set; } = 1;
        public string[] PeltCount { get; set; } = new string[4] { "1", "1", "1", "1" };
        public string[] FurPlateCount { get; set; } = new string[4] { "1", "1", "1", "1" };
        public FormKey Meat { get; set; } = new();
        public List<Dictionary<FormKey, int>> Mats = new();
        public List<FormKey> NegativeTreasure { get; set; } = new();
        public FormKey SharedDeathItems { get; set; } = new();
        public FormKey BloodType { get; set; } = new();
        public FormKey Venom { get; set; } = new();
        public FormKey Voice { get; set; } = new();
    }
}
