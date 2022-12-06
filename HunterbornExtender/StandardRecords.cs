namespace HunterbornExtender;
using System;
using System.Collections.Generic;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using HunterbornExtender.Settings;
using static HunterbornExtender.FormKeys;
using Mutagen.Bethesda.Plugins.Records;

#pragma warning disable IDE1006 // Naming Styles


/// <summary>
/// The quest and formlists needed by the patching methods, resolved and ready.
/// An instance of StandardRecords is used by almost every method of the patcher.
/// 
/// </summary>
readonly record struct StandardRecords(
    Settings.Settings Settings,
    bool CacoInstalled,
    bool LastSeedInstalled,
    Func<IFormLinkGetter<IItemGetter>, IFormLinkGetter<IItemGetter>> FormLinkSubstitution,
    Func<IItemGetter, IItemGetter> ItemSubstitution,
    Quest _DS_Hunterborn,
    ILinkCache<ISkyrimMod, ISkyrimModGetter> LinkCache,
    ISkyrimMod PatchMod,
    AnimalClass<FormList> Animals,
    MonsterClass<FormList> Monsters,
    List<CreatureClass<FormList>> CreatureClasses)
{

    /// <summary>
    /// Retrieves the CreatureClass for a specified Plugins.
    /// </summary>
    /// <param internalName="d">The Plugins whose CreatureClass should be returned.</param>
    /// <returns>The CreatureClass.</returns>
    public CreatureClass<FormList> GetCCFor(Program.CreatureData d) => GetCCFor(d.Prototype.Type);

    /// <summary>
    /// Retrieves the CreatureClass for a specified EntryType.
    /// </summary>
    /// <param internalName="d">The EntryType whose CreatureClass should be returned.</param>
    /// <returns>The CreatureClass.</returns>
    public CreatureClass<FormList> GetCCFor(EntryType t) => t switch
    {
        EntryType.Animal => Animals,
        EntryType.Monster => Monsters,
        _ => throw new InvalidOperationException("Unknown CreatureClass requested."),
    };

    /// <summary>
    /// Some of the methods in this class require the resolved Hunterborn Quest and resolved FormLists.
    /// This resolves them, adds them to the patch, and packs them into a Record.
    /// 
    /// Stores the Settings object too.
    /// </summary>
    static public StandardRecords CreateInstance(IPatcherState<ISkyrimMod, ISkyrimModGetter> state, Settings.Settings settings)
    {
        Quest hunterbornQuest = state.PatchMod.Quests.GetOrAddAsOverride(FormKeys._DS_Hunterborn.Resolve<IQuestGetter>(state.LinkCache));
        //if (settings.DebuggingMode) QueryImportantProperties(hunterbornQuest);

        var animals = new AnimalClass<FormList>(new(
                    state.PatchMod.FormLists.GetOrAddAsOverride(_DS_FL_Mats__Lists.Resolve(state.LinkCache) ?? throw new CoreRecordMissing(_DS_FL_Mats__Lists)),
                    state.PatchMod.FormLists.GetOrAddAsOverride(_DS_FL_Mats__Perfect.Resolve(state.LinkCache) ?? throw new CoreRecordMissing(_DS_FL_Mats__Perfect)),
                    state.PatchMod.FormLists.GetOrAddAsOverride(_DS_FL_PeltLists.Resolve(state.LinkCache) ?? throw new CoreRecordMissing(_DS_FL_PeltLists)),
                    state.PatchMod.FormLists.GetOrAddAsOverride(_DS_FL_DeathItems.Resolve(state.LinkCache) ?? throw new CoreRecordMissing(_DS_FL_DeathItems)),
                    state.PatchMod.FormLists.GetOrAddAsOverride(_DS_FL_DeathItemTokens.Resolve(state.LinkCache) ?? throw new CoreRecordMissing(_DS_FL_DeathItemTokens)),

                    GetProperty<ScriptObjectListProperty>(hunterbornQuest, "_DS_HB_Animals", "ActiveAnimalSwitches"),
                    GetProperty<ScriptObjectListProperty>(hunterbornQuest, "_DS_HB_Animals", "DeathItemLI"),
                    GetProperty<ScriptObjectListProperty>(hunterbornQuest, "_DS_HB_Animals", "MeatTypes"),
                    GetProperty<ScriptObjectListProperty>(hunterbornQuest, "_DS_HB_Animals", "SharedDeathItems"),
                    GetProperty<ScriptStringListProperty>(hunterbornQuest, "_DS_HB_Animals", "AnimalIndex"),
                    GetProperty<ScriptFloatListProperty>(hunterbornQuest, "_DS_HB_Animals", "AllMeatWeights"),
                    GetProperty<ScriptIntListProperty>(hunterbornQuest, "_DS_HB_Animals", "DefaultPeltValues"),
                    GetProperty<ScriptIntListProperty>(hunterbornQuest, "_DS_HB_Animals", "CarcassSizes")),
                    GetProperty<ScriptObjectListProperty>(hunterbornQuest, "_DS_HB_MAIN", "FreshCarcassMsgBoxes"),
                    state.PatchMod.FormLists.GetOrAddAsOverride(_DS_FL_CarcassObjects.Resolve(state.LinkCache) ?? throw new CoreRecordMissing(_DS_FL_CarcassObjects)));

        var monsters = new MonsterClass<FormList>(new(
                    state.PatchMod.FormLists.GetOrAddAsOverride(_DS_FL_Mats__Lists_Monsters.Resolve(state.LinkCache) ?? throw new CoreRecordMissing(_DS_FL_Mats__Lists_Monsters)),
                    state.PatchMod.FormLists.GetOrAddAsOverride(_DS_FL_Mats__Perfect_Monsters.Resolve(state.LinkCache) ?? throw new CoreRecordMissing(_DS_FL_Mats__Perfect_Monsters)),
                    state.PatchMod.FormLists.GetOrAddAsOverride(_DS_FL_PeltLists_Monsters.Resolve(state.LinkCache) ?? throw new CoreRecordMissing(_DS_FL_PeltLists_Monsters)),
                    state.PatchMod.FormLists.GetOrAddAsOverride(_DS_FL_DeathItems_Monsters.Resolve(state.LinkCache) ?? throw new CoreRecordMissing(_DS_FL_DeathItems_Monsters)),
                    state.PatchMod.FormLists.GetOrAddAsOverride(_DS_FL_DeathItemTokens_Monsters.Resolve(state.LinkCache) ?? throw new CoreRecordMissing(_DS_FL_DeathItemTokens_Monsters)),

                    GetProperty<ScriptObjectListProperty>(hunterbornQuest, "_DS_HB_Monsters", "ActiveMonsterSwitches"),
                    GetProperty<ScriptObjectListProperty>(hunterbornQuest, "_DS_HB_Monsters", "DeathItemLI"),
                    GetProperty<ScriptObjectListProperty>(hunterbornQuest, "_DS_HB_Monsters", "MeatTypes"),
                    GetProperty<ScriptObjectListProperty>(hunterbornQuest, "_DS_HB_Monsters", "SharedDeathItems"),
                    GetProperty<ScriptStringListProperty>(hunterbornQuest, "_DS_HB_Monsters", "MonsterIndex"),
                    GetProperty<ScriptFloatListProperty>(hunterbornQuest, "_DS_HB_Monsters", "AllMeatWeights"),
                    GetProperty<ScriptIntListProperty>(hunterbornQuest, "_DS_HB_Monsters", "DefaultPeltValues"),
                    GetProperty<ScriptIntListProperty>(hunterbornQuest, "_DS_HB_Monsters", "CarcassSizes")),
                    GetProperty<ScriptObjectListProperty>(hunterbornQuest, "_DS_HB_Monsters", "BloodTypes"),
                    GetProperty<ScriptObjectListProperty>(hunterbornQuest, "_DS_HB_Monsters", "VenomTypes"),
                    GetProperty<ScriptObjectListProperty>(hunterbornQuest, "_DS_HB_Monsters", "NegativeTreasure"));


        StandardRecords std = new(
            settings,
            state.LoadOrder.ContainsKey(CACO_MODKEY),
            state.LoadOrder.ContainsKey(LASTSEED_MODKEY),
            Substitutions.GetCACOSub(state.LoadOrder.ContainsKey(CACO_MODKEY)),
            Substitutions.GetCACOSubResolved(state.LoadOrder.ContainsKey(CACO_MODKEY), state.LinkCache),
            hunterbornQuest,
            state.LinkCache,
            state.PatchMod,
            animals,
            monsters,
            new() { animals, monsters });

        return std;
    }

    static ScriptTProperty GetProperty<ScriptTProperty>(IQuestGetter quest, String scriptName, String propertyName) where ScriptTProperty : ScriptProperty
    {
        String questName = quest.Name?.ToString() ?? quest.EditorID ?? "Quest";

        var scriptFilter = ScriptFilter(scriptName);
        var propertyFilter = PropertyFilter(propertyName);

        var script = quest.VirtualMachineAdapter?.Scripts.Where(scriptFilter).FirstOrDefault();
        if (script == null) throw new ScriptMissing(questName, scriptName);

        if (script.Properties.Where(propertyFilter).FirstOrDefault() is not ScriptTProperty property)
            throw new PropertyMissing(questName, scriptName, propertyName);

        return property;
    }

    /// <summary>
    /// A predicate for matching scripts by internalName.
    /// </summary>
    /// <param internalName="name">The internalName to match.</param>
    /// <returns>A predicate that matches the specified internalName.</returns>
    /// 
    static Func<IScriptEntryGetter, bool> ScriptFilter(String name) => (IScriptEntryGetter s) => name.EqualsIgnoreCase(s?.Name);

    /// <summary>
    /// A predicate for matching script properties by internalName.
    /// </summary>
    /// <param internalName="name">The internalName to match.</param>
    /// <returns>A predicate that matches the specified internalName.</returns>
    /// 
    static Func<IScriptPropertyGetter, bool> PropertyFilter(String name) => (IScriptPropertyGetter s) => name.EqualsIgnoreCase(s?.Name);

}

/// <summary>
/// The quest and formlists from the parent plugin, resolved and ready to go.
/// </summary>
readonly record struct ParentRecords(
    IQuestGetter _DS_Hunterborn,
    AnimalClass<IFormListGetter> Animals,
    MonsterClass<IFormListGetter> Monsters,
    List<CreatureClass<IFormListGetter>> CreatureClasses)
{

    /// <summary>
    /// Retrieves the CreatureClass for a specified Plugins.
    /// </summary>
    /// <param internalName="d">The Plugins whose CreatureClass should be returned.</param>
    /// <returns>The CreatureClass.</returns>
    public CreatureClass<IFormListGetter> GetCCFor(Program.CreatureData d) => GetCCFor(d.Prototype.Type);

    /// <summary>
    /// Retrieves the CreatureClass for a specified EntryType.
    /// </summary>
    /// <param internalName="d">The EntryType whose CreatureClass should be returned.</param>
    /// <returns>The CreatureClass.</returns>
    public CreatureClass<IFormListGetter> GetCCFor(EntryType t) => t switch
    {
        EntryType.Animal => Animals,
        EntryType.Monster => Monsters,
        _ => throw new InvalidOperationException("Unknown CreatureClass requested."),
    };
}

/// <summary>
/// FormLists and script properties needed for patching creatures.
/// Parameterized by whether it contains FormLists (for patching) or IFormListGetter 
/// (for examining the parent plugin's records).
/// </summary>
record CreatureClass<T>(
    T _DS_FL_Mats__Lists,
    T _DS_FL_Mats__Perfect,
    T _DS_FL_PeltLists,
    T _DS_FL_DeathItems,
    T _DS_FL_DeathItemTokens,
    ScriptObjectListProperty Switches,
    ScriptObjectListProperty DeathDescriptors,
    ScriptObjectListProperty MeatType,
    ScriptObjectListProperty SharedDeathItems,
    ScriptStringListProperty RaceIndex,
    ScriptFloatListProperty MeatWeights,
    ScriptIntListProperty PeltValues,
    ScriptIntListProperty CarcassSizes
    ) where T : IFormListGetter;

/// <summary>
/// FormLists and script properties needed for patching Animals,
/// which is just the FormList of carcasses.
/// </summary>
record AnimalClass<T>(
    CreatureClass<T> proto,
    ScriptObjectListProperty CarcassMessages,
    T _DS_FL_CarcassObjects) : CreatureClass<T>(proto) where T : IFormListGetter;

/// <summary>
/// FormLists and script properties needed for patching Monsters,
/// which includes blood, venom, and discards.
/// </summary>
record MonsterClass<T>(
    CreatureClass<T> proto,
    ScriptObjectListProperty BloodItems,
    ScriptObjectListProperty VenomItems,
    ScriptObjectListProperty Discards) : CreatureClass<T>(proto) where T : IFormListGetter;
