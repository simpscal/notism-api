using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

namespace Notism.Domain.BlogMedia.Specifications;

public class BlogMediaByMediaAssetIdSpecification : Specification<BlogMedia>
{
    private readonly Guid _mediaAssetId;

    public BlogMediaByMediaAssetIdSpecification(Guid mediaAssetId)
    {
        _mediaAssetId = mediaAssetId;
    }

    public override Expression<Func<BlogMedia, bool>> ToExpression()
    {
        return bm => bm.MediaAssetId == _mediaAssetId && !bm.IsDeleted;
    }
}

