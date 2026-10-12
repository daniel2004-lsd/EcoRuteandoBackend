using EcoRuteando.Modules.Admin.Application.Analytics.Dtos;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.Analytics.Queries.GetStatisticsSummary;

public record GetStatisticsSummaryQuery : IRequest<StatisticsSummaryDto>;
