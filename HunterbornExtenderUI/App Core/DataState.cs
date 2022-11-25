using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HunterbornExtenderUI
{
    public class DataState
    {
        public HashSet<Plugin> Plugins { get; set; } = new();
        public DataState()
        {

        }
    }
}
