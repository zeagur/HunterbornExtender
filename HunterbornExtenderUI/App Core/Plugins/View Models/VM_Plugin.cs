using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HunterbornExtenderUI
{
    public class VM_Plugin : VM
    {
        private StateProvider _state;
        public VM_Plugin(StateProvider state)
        {
            _state = state;

            AddEntry = new RelayCommand(
                canExecute: _ => true,
                execute: _ => Entries.Add(new VM_PluginEntry(_state))
            );

            DeleteEntry = new RelayCommand(
                canExecute: _ => true,
                execute: x => Entries.Remove((VM_PluginEntry)x)
            );
        }

        public ObservableCollection<VM_PluginEntry> Entries { get; set; } = new();
        public VM_PluginEntry? DisplayedEntry { get; set; }
        public Plugin SourceDTO { get; set; } = new();
        public string FilePath { get; set; } = "";
        public string FileName { get; set; } = "";
        public RelayCommand AddEntry { get; }
        public RelayCommand DeleteEntry { get; }

        public void LoadFromModel(Plugin model)
        {
            SourceDTO = model;
            foreach (var entry in model.Entries)
            {
                var viewModel = new VM_PluginEntry(_state);
                viewModel.LoadFromModel(entry);
                Entries.Add(viewModel);
            }
            FilePath = model.FilePath;
            FileName = Path.GetFileName(FilePath);
        }

        public Plugin DumpToModel()
        {
            SourceDTO.Entries.Clear();
            foreach (var entry in Entries)
            {
                SourceDTO.Entries.Add(entry.DumpToModel());
            }
            return SourceDTO;
        }
    }
}
