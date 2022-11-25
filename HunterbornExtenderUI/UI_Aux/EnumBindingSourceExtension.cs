using System;
using System.Windows.Markup;

namespace HunterbornExtenderUI;

public class EnumBindingSourceExtension : MarkupExtension // https://brianlagunas.com/a-better-way-to-data-bind-enums-in-wpf/
{
    private Type _enumType;
    public Type EnumType
    {
        get { return this._enumType; }
        set
        {
            if (value != this._enumType)
            {
                if (null != value)
                {
                    Type enumType = Nullable.GetUnderlyingType(value) ?? value;

                    if (!enumType.IsEnum)
                        throw new ArgumentException("Type must be for an Enum.");
                }

#pragma warning disable CS8601 // Possible null reference assignment.
                this._enumType = value;
#pragma warning restore CS8601 // Possible null reference assignment.
            }
        }
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public EnumBindingSourceExtension() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public EnumBindingSourceExtension(Type enumType)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        this.EnumType = enumType;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (null == this._enumType)
            throw new InvalidOperationException("The EnumType must be specified.");

        Type actualEnumType = Nullable.GetUnderlyingType(this._enumType) ?? this._enumType;
        Array enumValues = Enum.GetValues(actualEnumType);

        if (actualEnumType == this._enumType)
            return enumValues;

        Array tempArray = Array.CreateInstance(actualEnumType, enumValues.Length + 1);
        enumValues.CopyTo(tempArray, 1);
        return tempArray;
    }
}