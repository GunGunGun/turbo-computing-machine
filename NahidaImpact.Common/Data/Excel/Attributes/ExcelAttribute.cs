﻿using NahidaImpact.Common.Data.Excel;

namespace NahidaImpact.Common.Data.Excel.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
internal class ExcelAttribute : Attribute
{
    public ExcelType Type { get; }
    public string AssetName { get; }

    public ExcelAttribute(ExcelType type, string assetName)
    {
        Type = type;
        AssetName = assetName;
    }
}
