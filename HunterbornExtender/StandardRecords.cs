namespace HunterbornExtender;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using HunterbornExtender.Settings;
using static HunterbornExtender.FormKeys;
using PatchingRecords = StandardRecords<Mutagen.Bethesda.Skyrim.ISkyrimMod, Mutagen.Bethesda.Skyrim.FormList>;
using ViewingRecords = StandardRecords<Mutagen.Bethesda.Skyrim.ISkyrimModGetter, Mutagen.Bethesda.Skyrim.IFormListGetter>;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Plugins.Cache;
using static HunterbornExtender.ScriptUtil;
#pragma warning disable IDE1006 // Naming Styles


/// <summary>
/// The quest, formlists, and script properties needed by the patching methods, already 
/// resolved and ready.
/// 
/// An instance of StandardRecords is used by almost every method of the patcher.
/// 
/// It's parameterized on the type of mod and formList interfaces it uses. 
/// The read-only version uses ISkyrimModGetter and IFormListGetter.
/// The patching version uses SkyrimMod and FormList.
/// 
/// </summary>
readonly record struct StandardRecords<PatchType, FormListType>(
    IQuestGetter _DS_Hunterborn,
    AnimalClass<FormListType> Animals,
    MonsterClass<FormListType> Monsters,
    List<CreatureClass<FormListType>> CreatureClasses) where PatchType : ISkyrimModGetter where FormListType : IFormListGetter
{

    /// <summary>
    /// Retrieves the CreatureClass for a specified Plugins.
    /// </summary>
    /// <param internalName="d">The Plugins whose CreatureClass should be returned.</param>
    /// <returns>The CreatureClass.</returns>
    public CreatureClass<FormListType> GetCCFor(Program.CreatureData d) => GetCCFor(d.Prototype.Type);

    /// <summary>
    /// Retrieves the CreatureClass for a specified EntryType.
    /// </summary>
    /// <param internalName="d">The EntryType whose CreatureClass should be returned.</param>
    /// <returns>The CreatureClass.</returns>
    public CreatureClass<FormListType> GetCCFor(EntryType t) => t switch
    {
        EntryType.Animal => Animals,
        EntryType.Monster => Monsters,
        _ => throw new InvalidOperationException("Unknown CreatureClass requested."),
    };

    /// <summary>
    /// A patching form of StandardRecords, which creates overrides and new records in PatchMod.
    /// </summary>
    static public PatchingRecords CreatePatchingInstance(ISkyrimMod patchMod, ILoadOrder<IModListing<ISkyrimModGetter>> loadOrder, ILinkCache<ISkyrimMod, ISkyrimModGetter> linkCache)
    {
        Quest hunterbornQuest = patchMod.Quests.GetOrAddAsOverride(FormKeys._DS_Hunterborn.Resolve<IQuestGetter>(linkCache));

        //if (settings.DebuggingMode) QueryImportantProperties(hunterbornQuest);

        var animals = new AnimalClass<FormList>(new(
                    patchMod.FormLists.GetOrAddAsOverride(_DS_FL_Mats__Lists.Resolve(linkCache) ?? throw new CoreRecordMissing(_DS_FL_Mats__Lists)),
                    patchMod.FormLists.GetOrAddAsOverride(_DS_FL_Mats__Perfect.Resolve(linkCache) ?? throw new CoreRecordMissing(_DS_FL_Mats__Perfect)),
                    patchMod.FormLists.GetOrAddAsOverride(_DS_FL_PeltLists.Resolve(linkCache) ?? throw new CoreRecordMissing(_DS_FL_PeltLists)),
                    patchMod.FormLists.GetOrAddAsOverride(_DS_FL_DeathItems.Resolve(linkCache) ?? throw new CoreRecordMissing(_DS_FL_DeathItems)),
                    patchMod.FormLists.GetOrAddAsOverride(_DS_FL_DeathItemTokens.Resolve(linkCache) ?? throw new CoreRecordMissing(_DS_FL_DeathItemTokens)),

                    GetProperty<ScriptObjectListProperty>(hunterbornQuest, "_DS_HB_Animals", "ActiveAnimalSwitches"),
                    GetProperty<ScriptObjectListProperty>(hunterbornQuest, "_DS_HB_Animals", "DeathItemLI"),
                    GetProperty<ScriptObjectListProperty>(hunterbornQuest, "_DS_HB_Animals", "MeatTypes"),
                    GetProperty<ScriptObjectListProperty>(hunterbornQuest, "_DS_HB_Animals", "SharedDeathItems"),
                    GetProperty<ScriptStringListProperty>(hunterbornQuest, "_DS_HB_Animals", "AnimalIndex"),
                    GetProperty<ScriptFloatListProperty>(hunterbornQuest, "_DS_HB_Animals", "AllMeatWeights"),
                    GetProperty<ScriptIntListProperty>(hunterbornQuest, "_DS_HB_Animals", "DefaultPeltValues"),
                    GetProperty<ScriptIntListProperty>(hunterbornQuest, "_DS_HB_Animals", "CarcassSizes")),
                    GetProperty<ScriptObjectListProperty>(hunterbornQuest, "_DS_HB_MAIN", "FreshCarcassMsgBoxes"),
                    patchMod.FormLists.GetOrAddAsOverride(_DS_FL_CarcassObjects.Resolve(linkCache) ?? throw new CoreRecordMissing(_DS_FL_CarcassObjects)));

        var monsters = new MonsterClass<FormList>(new(
                    patchMod.FormLists.GetOrAddAsOverride(_DS_FL_Mats__Lists_Monsters.Resolve(linkCache) ?? throw new CoreRecordMissing(_DS_FL_Mats__Lists_Monsters)),
                    patchMod.FormLists.GetOrAddAsOverride(_DS_FL_Mats__Perfect_Monsters.Resolve(linkCache) ?? throw new CoreRecordMissing(_DS_FL_Mats__Perfect_Monsters)),
                    patchMod.FormLists.GetOrAddAsOverride(_DS_FL_PeltLists_Monsters.Resolve(linkCache) ?? throw new CoreRecordMissing(_DS_FL_PeltLists_Monsters)),
                    patchMod.FormLists.GetOrAddAsOverride(_DS_FL_DeathItems_Monsters.Resolve(linkCache) ?? throw new CoreRecordMissing(_DS_FL_DeathItems_Monsters)),
                    patchMod.FormLists.GetOrAddAsOverride(_DS_FL_DeathItemTokens_Monsters.Resolve(linkCache) ?? throw new CoreRecordMissing(_DS_FL_DeathItemTokens_Monsters)),

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

        return new(
            hunterbornQuest,
            animals,
            monsters,
            new() { animals, monsters });
    }

    /// <summary>
    /// A read-only form of StandardRecords, for pre-processing.
    /// </summary>
    static public ViewingRecords CreateViewingInstance(ILinkCache<ISkyrimMod, ISkyrimModGetter> linkCache)
    {
        IQuestGetter hunterbornQuest = FormKeys._DS_Hunterborn.Resolve<IQuestGetter>(linkCache);
        //if (settings.DebuggingMode) QueryImportantProperties(hunterbornQuest);

        var animals = new AnimalClass<IFormListGetter>(new(
                    _DS_FL_Mats__Lists.Resolve(linkCache),
                    _DS_FL_Mats__Perfect.Resolve(linkCache),
                    _DS_FL_PeltLists.Resolve(linkCache),
                    _DS_FL_DeathItems.Resolve(linkCache),
                    _DS_FL_DeathItemTokens.Resolve(linkCache),

                    GetProperty<ScriptObjectListProperty>(hunterbornQuest, "_DS_HB_Animals", "ActiveAnimalSwitches"),
                    GetProperty<ScriptObjectListProperty>(hunterbornQuest, "_DS_HB_Animals", "DeathItemLI"),
                    GetProperty<ScriptObjectListProperty>(hunterbornQuest, "_DS_HB_Animals", "MeatTypes"),
                    GetProperty<ScriptObjectListProperty>(hunterbornQuest, "_DS_HB_Animals", "SharedDeathItems"),
                    GetProperty<ScriptStringListProperty>(hunterbornQuest, "_DS_HB_Animals", "AnimalIndex"),
                    GetProperty<ScriptFloatListProperty>(hunterbornQuest, "_DS_HB_Animals", "AllMeatWeights"),
                    GetProperty<ScriptIntListProperty>(hunterbornQuest, "_DS_HB_Animals", "DefaultPeltValues"),
                    GetProperty<ScriptIntListProperty>(hunterbornQuest, "_DS_HB_Animals", "CarcassSizes")),
                    GetProperty<ScriptObjectListProperty>(hunterbornQuest, "_DS_HB_MAIN", "FreshCarcassMsgBoxes"),
                    _DS_FL_CarcassObjects.Resolve(linkCache));

        var monsters = new MonsterClass<IFormListGetter>(new(
                    _DS_FL_Mats__Lists_Monsters.Resolve(linkCache),
                    _DS_FL_Mats__Perfect_Monsters.Resolve(linkCache),
                    _DS_FL_PeltLists_Monsters.Resolve(linkCache),
                    _DS_FL_DeathItems_Monsters.Resolve(linkCache),
                    _DS_FL_DeathItemTokens_Monsters.Resolve(linkCache),

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


        return new(
            hunterbornQuest,
            animals,
            monsters,
            new() { animals, monsters });
    }

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
