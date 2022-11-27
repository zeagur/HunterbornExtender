using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using System.Collections.ObjectModel;

namespace HunterbornExtenderUI
{
    public class VM_DeathItemSelection
    {
        private StateProvider _state;
        public ILeveledItemGetter? DeathItemList { get; set; }
        public string CreatureEntryName { get; set; } = String.Empty;
        public VM_DeathItemAssignmentPage ParentMenu { get; set; }
        public ObservableCollection<INpcGetter> AssignedNPCs { get; set; } = new();

        public VM_DeathItemSelection(StateProvider state, VM_DeathItemAssignmentPage parentMenu)
        {
            _state = state;
            ParentMenu = parentMenu;
            DeathItemList = null;
        }

        public void LoadFromModel(DeathItemSelection model)
        {
            if (_state.LinkCache.TryResolve<ILeveledItemGetter>(model.DeathItemList, out var deathItemList))
            {
                DeathItemList = deathItemList;
                CreatureEntryName = ParentMenu.CreatureTypes.Where(x => x.Equals(model.CreatureEntryName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault() ?? "Skip";
                AssignedNPCs = new(model.AssignedNPCs);
            }
        }

        public DeathItemSelection DumpToModel()
        {
            DeathItemSelection model = new();
            model.DeathItemList = DeathItemList?.FormKey ?? FormKey.Null;
            model.CreatureEntryName = CreatureEntryName;
            return model;
        }
    }
}
