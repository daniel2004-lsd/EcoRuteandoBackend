namespace EcoRuteando.Modules.Admin.Presentation.Contracts.Requests;

public sealed record CompensateCompanyCarbonRequest(
    decimal Co2CompensatedKg,
    string? PartnerProgram = null,
    string? Notes = null);
