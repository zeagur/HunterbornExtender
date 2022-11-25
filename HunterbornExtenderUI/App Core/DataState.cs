using DynamicData;

namespace HunterbornExtenderUI;

public class DataState
{
    public SourceList<Plugin> Plugins { get; set; } = new();
}