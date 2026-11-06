using EcoRuteando.Modules.Admin.Application.Sustainability.Dtos;
using EcoRuteando.Modules.Admin.Domain.Entities;
using EcoRuteando.Modules.Admin.Domain.Repositories;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.Sustainability.Commands.CompensateCompanyCarbon;

public sealed class CompensateCompanyCarbonCommandHandler
    : IRequestHandler<CompensateCompanyCarbonCommand, CompensateCompanyCarbonResponse>
{
    private readonly ICompanyRepository _companyRepository;
    private readonly ICarbonCompensationRepository _compensationRepository;
    private readonly IAdminUnitOfWork _unitOfWork;

    public CompensateCompanyCarbonCommandHandler(
        ICompanyRepository companyRepository,
        ICarbonCompensationRepository compensationRepository,
        IAdminUnitOfWork unitOfWork)
    {
        _companyRepository = companyRepository;
        _compensationRepository = compensationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CompensateCompanyCarbonResponse> Handle(
        CompensateCompanyCarbonCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Co2CompensatedKg <= 0)
        {
            throw new DomainException("La cantidad de CO2 a compensar debe ser mayor a cero.");
        }

        var company = await _companyRepository.GetByIdAsync(request.CompanyId, cancellationToken);
        if (company is null)
        {
            throw new NotFoundException($"La empresa con ID '{request.CompanyId}' no existe.");
        }

        var partnerProgram = string.IsNullOrWhiteSpace(request.PartnerProgram)
            ? "Reforestación Cuenca Río Las Ceibas (CAM Huila)"
            : request.PartnerProgram.Trim();

        // 1. Crear la entidad de compensación voluntaria en el dominio
        var compensation = CarbonCompensation.CreateVoluntary(
            companyId: company.Id,
            userId: null,
            co2CompensatedKg: request.Co2CompensatedKg,
            partnerProgram: partnerProgram,
            notes: request.Notes);

        // 2. Persistir en base de datos
        await _compensationRepository.AddAsync(compensation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 3. Retornar respuesta enriquecida para generación de certificado oficial
        return new CompensateCompanyCarbonResponse(
            CompensationId: compensation.Id,
            CompanyId: company.Id,
            CompanyName: company.BusinessName,
            Co2CompensatedKg: compensation.Co2CompensatedKg,
            TreesEquivalent: compensation.TreesEquivalent,
            PartnerProgram: compensation.PartnerProgram,
            CertificateCode: compensation.CertificateCode,
            CompensationStatus: compensation.CompensationStatus,
            CertifiedAt: compensation.CertifiedAt);
    }
}
