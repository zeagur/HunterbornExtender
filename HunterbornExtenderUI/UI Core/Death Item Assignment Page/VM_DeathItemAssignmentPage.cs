using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
using Noggog.WPF;

namespace HunterbornExtenderUI;

public class VM_DeathItemAssignmentPage : ViewModel
{
    private StateProvider _stateProvider;

    public List<string> DeathItemTypes { get; set; } = new();
    [Reactive]
    public ObservableCollection<VM_CreatureEntry> DeathItemCreatureEntries { get; set; } = new();
    public VM_DeathItemAssignmentPage(StateProvider stateProvider)
    {
        _stateProvider = stateProvider;

        DeathItemTypes = new List<string>(CreatureTypes.AnimalTypes);
        DeathItemTypes.AddRange(CreatureTypes.MonsterTypes);
        DeathItemTypes.Sort();
        DeathItemTypes.Insert(0, "Skip");

        ScanForNamedNPCs();
    }
        
    private void ScanForNamedNPCs()
    {

    }
}