using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;

namespace HunterbornExtenderUI;

public class EDIDtoForm
{
    private IStateProvider _state;
    public EDIDtoForm(IStateProvider state)
    {
        _state = state;
    }
    public FormKey? GetFormFromEditorID<TMajor>(string editorID) where TMajor : class, IMajorRecordGetter
    {
        if (editorID == null || editorID == String.Empty)
        {
            return null;
        }

        var x = _state.LoadOrder.PriorityOrder.WinningOverrides<TMajor>().Where(x => x.EditorID == editorID).FirstOrDefault();
        if (x != null)
        {
            return x.FormKey;
        }
        else
        {
            return null;
        }
    }
}