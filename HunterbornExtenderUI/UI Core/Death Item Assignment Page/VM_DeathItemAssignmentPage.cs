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
using HunterbornExtender;
using Mutagen.Bethesda.Plugins.Cache;
using static System.Diagnostics.DebuggableAttribute;
using System.Collections.Generic;

namespace HunterbornExtenderUI;

sealed public class VM_DeathItemAssignmentPage : ViewModel
{
    private StateProvider _stateProvider;
    private VM_PluginList _pluginVMList;
    private ObservableCollection<PluginEntry> _pluginEntries = new();
    private readonly SettingsProvider _settingsProvider;
    private readonly VMLoader_DeathItems _vmDeathItemLoader;

    [Reactive]
    public ObservableCollection<string> CreatureTypes { get; set; } = new();

    [Reactive]
    public VM_DeathItemSelectionList DeathItemSelectionList { get; set; }
    public VM_DeathItemAssignmentPage(StateProvider stateProvider, SettingsProvider settingsProvider, VM_PluginList pluginList, VM_DeathItemSelectionList deathItemList, DeathItemSettingsLoader deathItemSettingsLoader, VMLoader_DeathItems deathItemVMLoader)
    {
        _stateProvider = stateProvider;
        _settingsProvider = settingsProvider;
        _pluginVMList = pluginList;
        DeathItemSelectionList = deathItemList;
        _vmDeathItemLoader = deathItemVMLoader;
    }

    public void Initialize()
    {
        ScanForPluginEntries();
        var deathItems = Heuristics.MakeHeuristicSelections(_stateProvider.LoadOrder.PriorityOrder.OnlyEnabledAndExisting().WinningOverrides<INpcGetter>(), _pluginEntries.ToList(), _settingsProvider.PatcherSettings.DeathItemSelections, _stateProvider.LinkCache);
        DeathItemSelectionList.DeathItems.SetTo(_vmDeathItemLoader.GetDeathItemVMs(deathItems).Where(x => x.DeathItemList != null));
    }
        
    private void ScanForPluginEntries()
    {
        _pluginEntries = new(RecreateInternal.RecreateInternalPluginsUI(_stateProvider.LinkCache));
        foreach (var pluginEntry in _pluginVMList.Plugins.SelectMany(x => x.Entries).Select(x => x.DumpToModel()))
        {
            _pluginEntries.Add(pluginEntry);
        }

        CreatureTypes = new(_pluginEntries.Select(x => x.SortName));
        CreatureTypes.Sort(x => x, false);
    }
}