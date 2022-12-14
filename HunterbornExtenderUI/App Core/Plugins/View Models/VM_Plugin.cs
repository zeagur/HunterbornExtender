using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Noggog.WPF;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SynthEBD;

namespace HunterbornExtenderUI;

public class VM_Plugin : ViewModel
{
    private IStateProvider _state;
    public VM_Plugin(IStateProvider state)
    {
        _state = state;

        AddEntry = ReactiveCommand.Create(
            () => Entries.Add(new VM_PluginEntry(_state)));

        DeleteEntry = ReactiveCommand.Create<VM_PluginEntry>(
            x => Entries.Remove(x));

        SavePlugin = ReactiveCommand.Create(
            () => SaveToDisk());
    }

    public ObservableCollection<VM_PluginEntry> Entries { get; set; } = new();
    [Reactive]
    public VM_PluginEntry? DisplayedEntry { get; set; }
    public Plugin SourceDTO { get; set; } = new();
    public string FilePath { get; set; } = "";
    [Reactive]
    public string FileName { get; set; } = "";
    public ICommand AddEntry { get; }
    public ICommand DeleteEntry { get; }
    public ICommand SavePlugin { get; }

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

    public void SaveToDisk()
    {
        var model = DumpToModel();
        string savePath = string.Empty;
        if (FilePath != String.Empty && File.Exists(FilePath))
        {
            savePath = FilePath;
        }
        else
        {
            System.Windows.Forms.SaveFileDialog dialog = new ();

            var pluginsPath = Path.Combine(_state.ExtraSettingsDataPath, "Plugins");

            if (pluginsPath != "")
            {
                dialog.InitialDirectory = pluginsPath;
            }

            dialog.Filter = "JSON|*.json";

            dialog.Title = "Select Save Location";

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                savePath = dialog.FileName;
                FilePath = savePath;
                FileName = Path.GetFileName(FilePath);
            }
        }

        if (savePath != string.Empty)
        {
            JSONhandler<Plugin>.SaveJSONFile(model, FilePath);
            MessageBox.Show("Saved to " + FilePath);
        }
    }
}