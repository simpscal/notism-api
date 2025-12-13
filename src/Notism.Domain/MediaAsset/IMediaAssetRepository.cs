using Notism.Domain.Common.Interfaces;

namespace Notism.Domain.MediaAsset;

public interface IMediaAssetRepository : IRepository<MediaAsset>
{
    MediaAsset Update(MediaAsset mediaAsset);
}