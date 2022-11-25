using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;

namespace HunterbornExtenderUI
{
    public class EDIDtoForm
    {
        private StateProvider _state;
        public EDIDtoForm(StateProvider state)
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
}
