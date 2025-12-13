using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

namespace Notism.Domain.EventMedia.Specifications;

public class EventMediaByMediaAssetIdSpecification : Specification<EventMedia>
{
    private readonly Guid _mediaAssetId;

    public EventMediaByMediaAssetIdSpecification(Guid mediaAssetId)
    {
        _mediaAssetId = mediaAssetId;
    }

    public override Expression<Func<EventMedia, bool>> ToExpression()
    {
        return em => em.MediaAssetId == _mediaAssetId && !em.IsDeleted;
    }
}

