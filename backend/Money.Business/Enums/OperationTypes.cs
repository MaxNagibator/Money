﻿using System.ComponentModel;

namespace Money.Business.Enums;

public enum OperationTypes
{
    /// <summary>
    /// Расходы.
    /// </summary>
    [Description("Расходы")]
    Costs = 1,

    /// <summary>
    /// Доходы.
    /// </summary>
    [Description("Доходы")]
    Income = 2,
}

public static class AttributeHelper
{
    public static string? DescriptionAttr<T>(this T source)
    {
        if (Equals(source, default(T)))
        {
            return null;
        }

        var fi = source!.GetType().GetField(source.ToString()!);

        if (fi == null)
        {
            return null;
        }

        var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

        if (attributes.Length > 0)
        {
            return attributes[0].Description;
        }

        return source.ToString();
    }
}
