using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HunterbornExtenderUI
{
    public class Plugin
    {
        public List<PluginEntry> Entries { get; set; } = new();
        public string FilePath { get; set; } = "";
    }
}
