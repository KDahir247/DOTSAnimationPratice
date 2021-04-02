using Unity.Animation;
using Unity.Entities;

public struct ShootReload_PlayClipBuffer : IBufferElementData
{
	public BlobAssetReference<Clip> Clip;
}