using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using System.Collections.ObjectModel;
using HunterbornExtender.Settings;
using System.Windows.Data;
using System.Globalization;
using Mutagen.Bethesda.Plugins.Aspects;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Strings;

namespace HunterbornExtenderUI
{
    sealed public class VM_DeathItemSelection
    {
        private IStateProvider _state;
        public ILeveledItemGetter? DeathItem { get; set; }
        public string CreatureEntryName { get; set; } = String.Empty;
        public VM_DeathItemAssignmentPage ParentMenu { get; set; }
        public ObservableCollection<INpcGetter> AssignedNPCs { get; set; } = new();

        public VM_DeathItemSelection(IStateProvider state, VM_DeathItemAssignmentPage parentMenu)
        {
            _state = state;
            ParentMenu = parentMenu;
            DeathItem = null;
        }

        public void LoadFromModel(DeathItemSelection model)
        {
            if (_state.LinkCache.TryResolve<ILeveledItemGetter>(model.DeathItem, out var deathItemList))
            {
                DeathItem = deathItemList;
                CreatureEntryName = model.Selection?.SortName ?? "Skip";
                AssignedNPCs = new(model.AssignedNPCs);
            }
        }

        public DeathItemSelection DumpToModel()
        {
            DeathItemSelection model = new();
            model.DeathItem = DeathItem?.FormKey ?? FormKey.Null;
            model.CreatureEntryName = CreatureEntryName;
            return model;
        }
    }

    [ValueConversion(typeof(ILeveledItemGetter), typeof(string))]
    public sealed class DeathItemConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ILeveledItemGetter deathItem)
            {
                return deathItem.EditorID ?? "NO EDITORID";
            }
            else
            {
                return "INVALID DEATHITEM";
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    [ValueConversion(typeof(ITranslatedStringGetter), typeof(string))]
    public sealed class NameConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is ITranslatedStringGetter name
                ? name.ToString() ?? "(blank name)"
                : "(blank name)";
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(IFormLinkGetter), typeof(string))]
    public sealed class RaceLinkNamer: IValueConverter
    {
        static public IStateProvider? State { get; set; }

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (State is not null && value is IFormLinkGetter<IRaceGetter> raceLink)
            {
                raceLink.TryResolve(State.LinkCache, out var race);
                return race?.Name?.ToString() ?? "(couldn't get race)";
            }
            else return "(couldn't resolve race)";
        }

        object IValueConverter.ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
