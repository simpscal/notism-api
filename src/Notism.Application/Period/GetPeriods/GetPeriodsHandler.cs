using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Interfaces;
using Notism.Domain.Common.Interfaces;
using Notism.Domain.MediaAsset;
using Notism.Domain.MediaAsset.Specifications;
using Notism.Domain.Period.Specifications;
using Notism.Shared.Extensions;

using PeriodEntity = Notism.Domain.Period.Period;

namespace Notism.Application.Period.GetPeriods;

public class GetPeriodsHandler : IRequestHandler<GetPeriodsRequest, List<GetPeriodsResponse>>
{
    private readonly IRepository<PeriodEntity> _periodRepository;
    private readonly IStorageService _storageService;
    private readonly ILogger<GetPeriodsHandler> _logger;

    public GetPeriodsHandler(
        IRepository<PeriodEntity> periodRepository,
        IStorageService storageService,
        ILogger<GetPeriodsHandler> logger)
    {
        _periodRepository = periodRepository;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<List<GetPeriodsResponse>> Handle(
        GetPeriodsRequest request,
        CancellationToken cancellationToken)
    {
        // Use specification with Include for MediaAsset and ordering
        var specification = new PublishedPeriodsSpecification();
        var periods = await _periodRepository.FilterByExpressionAsync(specification);

        var response = periods.Select(period =>
        {
            string? thumbnailUrl = null;

            // Access MediaAsset through navigation property (loaded via Include in same query)
            if (period.ThumbnailMediaAsset != null)
            {
                var storagePath = period.ThumbnailMediaAsset.StoragePath;
                thumbnailUrl = storagePath.IsValidUrl()
                    ? storagePath
                    : _storageService.GetPublicUrl(storagePath);
            }

            return new GetPeriodsResponse
            {
                Id = period.Id,
                Name = period.Name,
                StartYear = period.StartYear,
                EndYear = period.EndYear,
                Description = period.Description,
                ThumbnailUrl = thumbnailUrl,
                DisplayOrder = period.DisplayOrder,
            };
        }).ToList();

        _logger.LogInformation(
            "Retrieved {Count} periods in chronological order",
            response.Count);

        return response;
    }
}

