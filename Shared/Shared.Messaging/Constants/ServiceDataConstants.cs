using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Constants;

public static class ServiceDataConstants
{
    public static Guid TgInternalProfileTypeId = Guid.Parse("26d5b374-09ad-4050-87ac-77b1bfc9c12d");
    public static Guid RuExternalProfileTypeId = Guid.Parse("042acb92-3caf-42ef-b66e-c344f79813ae");

    public static Guid VisaDocumentTypeId = Guid.Parse("c4ec1b82-32d6-45ce-b5b8-e37efdeaf6ea");
    public static Guid StudentCardDocumentTypeId = Guid.Parse("ea5e5137-b1d2-47b1-9df4-92f6884d6727");
    public static Guid MedicalInsurranceDocumentTypeId = Guid.Parse("0f9f2625-e01c-4c52-b439-2e77dd2925ca");
    public static Guid MedicalCertificateDocumentTypeId = Guid.Parse("b3aa1bb3-a906-44de-b156-3fc5ee650792");

}
