using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Domain.Enums;

public enum SensitiveDataType
{
    General,
    MedicalData,
    IdentityDocument,
    ContactInfo,
    FinancialData,
    BiometricData
}
