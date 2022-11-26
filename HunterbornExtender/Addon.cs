using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mutagen.Bethesda.Skyrim;

namespace HunterbornExtender
{
    record Addon(
        CreatureClass Type,
        String Name,
        String ProperName,
        String DescriptiveName,
        IGlobalGetter? Toggle,
        IMessageGetter? CarcassMessage,
        int CarcassSize,
        int CarcassWeight,
        int CarcassValue,
        PeltCount? PeltCount,
        FurCount? FurCount,
        IItemGetter? Meat,
        Materials? Materials,
        IItemGetter[]? RemoveFromInventory,
        String? SharedDeathItems,
        IItemGetter? Blood,
        IIngestibleGetter? Venom,
        IVoiceTypeGetter? Voice
        );

    enum CreatureClass { Animal, Monster };
    record PeltCount(int lvl0, int lvl1, int lvl2, int lvl3);
    record FurCount(int lvl0, int lvl1, int lvl2, int lvl3);
    record Materials(Dictionary<IItemGetter, int> lvl0, Dictionary<IItemGetter, int> lvl1, Dictionary<IItemGetter, int> lvl2, Dictionary<IItemGetter, int> lvl3);

}
