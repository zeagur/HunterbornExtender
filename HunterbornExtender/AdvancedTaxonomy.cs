namespace HunterbornExtender;

using DynamicData;
using HunterbornExtender.Settings;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Exceptions;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using System;
using System.Collections.Generic;
using static HunterbornExtender.FormKeys;

internal sealed class AdvancedTaxonomy
{
    public AdvancedTaxonomy() {}

    public void AddCreature(PluginEntry plugin)
    {
        var name = Naming.PluginNameFB(plugin);
        if (plugin.Type == EntryType.Animal && !AnimalNames.Contains(name)) AnimalNames.Add(name);
        else if (plugin.Type == EntryType.Monster && !MonsterNames.Contains(name)) MonsterNames.Add(name);
    }

    public void Finalize(ISkyrimMod patchMod, ILinkCache<ISkyrimMod, ISkyrimModGetter> linkCache)
    {
        var animals = new List<(string Name, int Index)>();
        var monsters = new List<(string Name, int Index)>();

        for (var i = 0; i < AnimalNames.Count; i++)
        {
            animals.Add((AnimalNames[i], i));
        }

        for (var i = 0; i < MonsterNames.Count; i++)
        {
            monsters.Add((MonsterNames[i], i));
        }

        animals.Sort();
        monsters.Sort();

        linkCache.TryResolve<IMagicEffectGetter>(ADVANCED_TAXONOMY, out var baseRecord);
        if (baseRecord is not null && patchMod.MagicEffects.GetOrAddAsOverride(baseRecord) is MagicEffect advancedTaxonomy)
        {
            var propertyAnimalNames = ScriptUtil.GetProperty<ScriptStringListProperty>(advancedTaxonomy, "_DS_HB_mgef_AdvancedTaxonomy", "animalNames");
            var propertyMonsterNames = ScriptUtil.GetProperty<ScriptStringListProperty>(advancedTaxonomy, "_DS_HB_mgef_AdvancedTaxonomy", "monsterNames");
            var propertyAnimalMapping = ScriptUtil.GetProperty<ScriptIntListProperty>(advancedTaxonomy, "_DS_HB_mgef_AdvancedTaxonomy", "animalMapping");
            var propertyMonsterMapping = ScriptUtil.GetProperty<ScriptIntListProperty>(advancedTaxonomy, "_DS_HB_mgef_AdvancedTaxonomy", "monsterMapping");
            var propertyInitialized = ScriptUtil.GetProperty<ScriptBoolProperty>(advancedTaxonomy, "_DS_HB_mgef_AdvancedTaxonomy", "initialized");
            Write.Success(1, "Loaded script properties.");

            propertyAnimalNames.Data.Clear();
            propertyAnimalNames.Data.AddRange(animals.Select(p => p.Name).ToArray());
            Write.Success(1, "Added animal names.");

            propertyMonsterNames.Data.Clear();
            propertyMonsterNames.Data.AddRange(monsters.Select(p => p.Name).ToArray());
            Write.Success(1, "Added monster names.");

            propertyAnimalMapping.Data.Clear();
            propertyAnimalMapping.Data.AddRange(animals.Select(p => p.Index).ToArray());
            Write.Success(1, "Added animal sorting.");

            propertyMonsterMapping.Data.Clear();
            propertyMonsterMapping.Data.AddRange(monsters.Select(p => p.Index).ToArray());
            Write.Success(1, "Added monster sorting.");

            propertyInitialized.Data = true;
        }
        else
        {
            throw new MissingRecordException(ADVANCED_TAXONOMY, typeof(IMagicEffectGetter));
        }

    }

    static private void QuickCoSort<S, T>(List<S> mainList, List<T> coList, int first, int last) where S : IComparable<S>
    {
        (var left, var right) = (first, last);
        var pivot = mainList[(first + last) / 2];

        while (left <= right)
        {
            while (mainList[left].CompareTo(pivot) < 0) left++;
            while (mainList[right].CompareTo(pivot) > 0) right--;

            if (left <= right)
            {
                Swap(mainList, coList, left, right);
                left++;
                right--;
            }
        }

        if (first < right) QuickCoSort(mainList, coList, first, right);
        if (left < last) QuickCoSort(mainList, coList, left, last);

    }

    static private void Swap<S, T>(List<S> mainList, List<T> coList, int a, int b)
    {
        (mainList[b], mainList[a]) = (mainList[a], mainList[b]);
        (coList[b], coList[a]) = (coList[a], coList[b]);
    }

    private readonly List<string> AnimalNames = new();
    private readonly List<string> MonsterNames = new();

}
