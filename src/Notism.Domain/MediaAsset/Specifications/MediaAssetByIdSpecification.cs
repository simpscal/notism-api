using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

namespace Notism.Domain.MediaAsset.Specifications;

public class MediaAssetByIdSpecification : Specification<MediaAsset>
{
    private readonly Guid _mediaAssetId;

    public MediaAssetByIdSpecification(Guid mediaAssetId)
    {
        _mediaAssetId = mediaAssetId;
    }

    public override Expression<Func<MediaAsset, bool>> ToExpression()
    {
        return m => m.Id == _mediaAssetId && !m.IsDeleted;
    }
}

