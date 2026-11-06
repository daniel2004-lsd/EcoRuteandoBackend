namespace EcoRuteando.Modules.Admin.Application.Sustainability.Dtos;

public sealed record CompensateCompanyCarbonResponse(
    Guid CompensationId,
    Guid CompanyId,
    string CompanyName,
    decimal Co2CompensatedKg,
    int TreesEquivalent,
    string PartnerProgram,
    string CertificateCode,
    string CompensationStatus,
    DateTime CertifiedAt);
