using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Constants;

public static class ServiceDataConstants
{
    public static Guid TgInternalProfileTypeId = Guid.Parse("26d5b374-09ad-4050-87ac-77b1bfc9c12d");
    public static Guid RuExternalProfileTypeId = Guid.Parse("042acb92-3caf-42ef-b66e-c344f79813ae");

    public static Guid VisaDocumentTypeId = Guid.Parse("c4ec1b82-32d6-45ce-b5b8-e37efdeaf6ea");
    public static Guid CniDocumentTypeId = Guid.Parse("f57c6db1-01d3-443f-bd60-d63ee7d2b129");
    public static Guid ExtraitNaissanceDocumentTypeId = Guid.Parse("6186f940-2237-4212-8c6f-481f221540ea");
    public static Guid PassportDocumentTypeId = Guid.Parse("e6a834d1-439a-4764-9524-90f132dfcf3c");
    public static Guid StudentCardDocumentTypeId = Guid.Parse("ea5e5137-b1d2-47b1-9df4-92f6884d6727");
    public static Guid MedicalInsurranceDocumentTypeId = Guid.Parse("0f9f2625-e01c-4c52-b439-2e77dd2925ca");
    public static Guid MedicalCertificateDocumentTypeId = Guid.Parse("b3aa1bb3-a906-44de-b156-3fc5ee650792");

    public static readonly Guid ParcelTransportCategoryId = Guid.Parse("019248a0-6b3a-7182-9a84-7c6d9e0f1234");
}
