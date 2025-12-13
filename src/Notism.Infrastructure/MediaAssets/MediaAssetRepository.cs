using Notism.Domain.MediaAsset;
using Notism.Infrastructure.Common;

namespace Notism.Infrastructure.MediaAssets;

public class MediaAssetRepository : Repository<MediaAsset>, IMediaAssetRepository
{
    public MediaAssetRepository(AppDbContext appDbContext)
        : base(appDbContext)
    {
    }

    public MediaAsset Update(MediaAsset mediaAsset)
    {
        _dbSet.Update(mediaAsset);
        return mediaAsset;
    }
}