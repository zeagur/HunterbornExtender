using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
using Noggog.WPF;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda;
using DynamicData;
using Noggog;
using HunterbornExtenderUI.App_Core.Death_Item_Selection;
using HunterbornExtender.Settings;

namespace HunterbornExtenderUI;

public class VM_DeathItemAssignmentPage : ViewModel
{
    private StateProvider _stateProvider;
    private VM_PluginList _pluginList;
    private DeathItemSettingsLoader _deathItemSettingsLoader;
    private readonly VMLoader_DeathItems _vmDeathItemLoader;

    [Reactive]
    public ObservableCollection<string> CreatureTypes { get; set; } = new();

    [Reactive]
    public VM_DeathItemSelectionList DeathItemSelectionList { get; set; }
    public VM_DeathItemAssignmentPage(StateProvider stateProvider, VM_PluginList pluginList, VM_DeathItemSelectionList deathItemList, DeathItemSettingsLoader deathItemSettingsLoader, VMLoader_DeathItems deathItemVMLoader)
    {
        _stateProvider = stateProvider;
        _pluginList = pluginList;
        DeathItemSelectionList = deathItemList;
        _deathItemSettingsLoader = deathItemSettingsLoader;
        _vmDeathItemLoader = deathItemVMLoader;

        CreatureTypes = new ObservableCollection<string>(HunterbornExtenderUI.CreatureTypes.AnimalTypes);
        foreach (var monster in HunterbornExtenderUI.CreatureTypes.MonsterTypes)
        {
            if (!CreatureTypes.Contains(monster))
            {
                CreatureTypes.Add(monster);
            }
        }

        CreatureTypes.Sort(x => x, false);
        CreatureTypes.Insert(0, "Skip");
    }

    public void Initialize(DeathItemSelection[] deathItemSelections)
    {
        ScanForPluginEntries();
        ScanForDeathItems(deathItemSelections.ToList());
    }
        
    private void ScanForPluginEntries()
    {
        foreach (var plugin in _pluginList.Plugins)
        {
            foreach (var pluginEntry in plugin.Entries)
            {
                if (!CreatureTypes.Contains(pluginEntry.Name))
                {
                    CreatureTypes.Add(pluginEntry.Name);
                }
            }
        }
        CreatureTypes.Sort(x => x, false);
    }

    private void ScanForDeathItems(List<DeathItemSelection> deathItemSettings)
    {
        foreach (var npc in _stateProvider.LoadOrder.PriorityOrder.OnlyEnabledAndExisting().WinningOverrides<INpcGetter>().Where(x => x.DeathItem != null))
        {
            var deathItemListFormLink = npc.DeathItem;
            var existingEntry = deathItemSettings.Where(x => x.DeathItemList.Equals(deathItemListFormLink.FormKey)).FirstOrDefault();
            if (existingEntry != null) 
            {
                // Record NPC 
                existingEntry.AssignedNPCs.Add(npc);
            }
            else if (_stateProvider.LinkCache.TryResolve<ILeveledItemGetter>(deathItemListFormLink.FormKey, out var deathItemListGetter) && IsPatchableDeathItem(deathItemListGetter, out string correspondingCreatureName))
            {
                // Add new entry
                var newEntry = new DeathItemSelection();
                newEntry.DeathItemList = deathItemListFormLink.FormKey;
                newEntry.CreatureEntryName = correspondingCreatureName;
                newEntry.AssignedNPCs.Add(npc);
                deathItemSettings.Add(newEntry);
            }
        }

        DeathItemSelectionList.DeathItems.SetTo(_vmDeathItemLoader.GetDeathItemVMs(deathItemSettings).Where(x => x.DeathItemList != null));
    }

    private bool IsPatchableDeathItem(ILeveledItemGetter deathItem, out string creatureEntryName)
    {
        var edid = deathItem.EditorID;
        if (edid != null && edid.StartsWith("DeathItem", StringComparison.OrdinalIgnoreCase))
        {
            int splitPos = edid.IndexOf("DeathItem", StringComparison.OrdinalIgnoreCase);
            splitPos += "DeathItem".Length;
            string subName = edid.Substring(splitPos);

            foreach (var plugin in _pluginList.Plugins)
            {
                foreach (var entry in plugin.Entries)
                {
                    if (entry.Name.Contains(subName, StringComparison.OrdinalIgnoreCase))
                    {
                        creatureEntryName = entry.Name;
                        return true;
                    }
                }
            }
        }

        creatureEntryName = "";
        return false;
    }
}