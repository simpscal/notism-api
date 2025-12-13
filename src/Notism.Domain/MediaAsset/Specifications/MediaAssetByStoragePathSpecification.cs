using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

namespace Notism.Domain.MediaAsset.Specifications;

public class MediaAssetByStoragePathSpecification : Specification<MediaAsset>
{
    private readonly string _storagePath;

    public MediaAssetByStoragePathSpecification(string storagePath)
    {
        _storagePath = storagePath;
    }

    public override Expression<Func<MediaAsset, bool>> ToExpression()
    {
        return m => m.StoragePath == _storagePath && !m.IsDeleted;
    }
}

