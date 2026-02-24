using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
   
namespace Shared.Domain.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class SensitiveDataAttribute : Attribute
{
    public SensitiveDataType DataType { get; set; } = SensitiveDataType.General;
    public bool IsPersonalData { get; set; } = false;

    public SensitiveDataAttribute()
    {
    }

    public SensitiveDataAttribute(SensitiveDataType dataType, bool isPersonalData = false)
    {
        DataType = dataType;
        IsPersonalData = isPersonalData;
    }
}
