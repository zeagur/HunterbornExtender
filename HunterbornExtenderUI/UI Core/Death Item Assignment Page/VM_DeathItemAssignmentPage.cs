using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
using Noggog.WPF;

namespace HunterbornExtenderUI
{
    public class VM_DeathItemAssignmentPage : ViewModel
    {
        private StateProvider _stateProvider;
        private DataState _dataState;

        public List<string> DeathItemTypes { get; set; } = new();
        [Reactive]
        public ObservableCollection<VM_CreatureEntry> DeathItemCreatureEntries { get; set; } = new();
        public VM_DeathItemAssignmentPage(StateProvider stateProvider, DataState dataState)
        {
            _stateProvider = stateProvider;
            _dataState = dataState;

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
}
