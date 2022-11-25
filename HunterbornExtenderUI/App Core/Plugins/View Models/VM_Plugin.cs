using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using Noggog.WPF;
using ReactiveUI;

namespace HunterbornExtenderUI;

public class VM_Plugin : ViewModel
{
    private StateProvider _state;
    public VM_Plugin(StateProvider state)
    {
        _state = state;

        AddEntry = ReactiveCommand.Create(
            () => Entries.Add(new VM_PluginEntry(_state)));

        DeleteEntry = ReactiveCommand.Create<VM_PluginEntry>(
            x => Entries.Remove(x));
    }

    public ObservableCollection<VM_PluginEntry> Entries { get; set; } = new();
    public VM_PluginEntry? DisplayedEntry { get; set; }
    public Plugin SourceDTO { get; set; } = new();
    public string FilePath { get; set; } = "";
    public string FileName { get; set; } = "";
    public ICommand AddEntry { get; }
    public ICommand DeleteEntry { get; }

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