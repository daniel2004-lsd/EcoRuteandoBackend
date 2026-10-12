using EcoRuteando.Modules.Admin.Application.System.Dtos;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.System.Queries.GetSystemConfiguration;

public record GetSystemConfigurationQuery : IRequest<SystemConfigurationDto>;
