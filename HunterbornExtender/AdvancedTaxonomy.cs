namespace HunterbornExtender;
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
        if (plugin.Type == EntryType.Animal)
            AnimalNames.Add(plugin.SortName);
        else
            MonsterNames.Add(plugin.SortName);
    }

    public void AddCreature(Program.CreatureData creature)
    {
        if (creature.Prototype.Type == EntryType.Animal)
            AnimalNames.Add(creature.DescriptiveName);
        else
            MonsterNames.Add(creature.DescriptiveName);
    }

    public void Finalize(ISkyrimMod patchMod, ILinkCache<ISkyrimMod, ISkyrimModGetter> linkCache)
    {
        var animalMapping = new List<int>(Enumerable.Range(0, AnimalNames.Count));
        var monsterMapping = new List<int>(Enumerable.Range(0, MonsterNames.Count));
        QuickCoSort(AnimalNames, animalMapping, 0, AnimalNames.Count - 1);
        QuickCoSort(MonsterNames, monsterMapping, 0, MonsterNames.Count - 1);

        linkCache.TryResolve<IMagicEffectGetter>(ADVANCED_TAXONOMY, out var baseRecord);
        if (baseRecord is not null && patchMod.MagicEffects.GetOrAddAsOverride(baseRecord) is MagicEffect advancedTaxonomy)
        {
            var propertyAnimalNames = ScriptUtil.GetProperty<ScriptStringListProperty>(advancedTaxonomy, "_DS_HB_mgef_AdvancedTaxonomy", "animalNames");
            var propertyMonsterNames = ScriptUtil.GetProperty<ScriptStringListProperty>(advancedTaxonomy, "_DS_HB_mgef_AdvancedTaxonomy", "monsterNames");
            var propertyAnimalMapping = ScriptUtil.GetProperty<ScriptIntListProperty>(advancedTaxonomy, "_DS_HB_mgef_AdvancedTaxonomy", "animalMapping");
            var propertyMonsterMapping = ScriptUtil.GetProperty<ScriptIntListProperty>(advancedTaxonomy, "_DS_HB_mgef_AdvancedTaxonomy", "monsterMapping");
            var propertyInitialized = ScriptUtil.GetProperty<ScriptBoolProperty>(advancedTaxonomy, "_DS_HB_mgef_AdvancedTaxonomy", "initialized");

            propertyAnimalNames.Data.Clear();
            propertyAnimalNames.Data.AddRange(AnimalNames);

            propertyMonsterNames.Data.Clear();
            propertyMonsterNames.Data.AddRange(MonsterNames);

            propertyAnimalMapping.Data.Clear();
            propertyAnimalMapping.Data.AddRange(animalMapping);

            propertyMonsterMapping.Data.Clear();
            propertyMonsterMapping.Data.AddRange(monsterMapping);

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
