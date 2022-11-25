namespace HunterbornExtenderUI;

public class EntryTypeConverter
{
    public static EntryType EntryStringToEnum(string entry)
    {
        if (entry.Equals("monster", StringComparison.OrdinalIgnoreCase))
        {
            return EntryType.Monster;
        }
        else if (entry.Equals("animal", StringComparison.OrdinalIgnoreCase))
        {
            return EntryType.Animal;
        }
        else
        {
            return EntryType.Monster; // default to monster if conversion fails
        }
    }
}