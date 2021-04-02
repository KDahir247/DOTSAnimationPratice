using Unity.Animation;
using Unity.Entities;

public struct PlayerBlendClipBuffer : IBufferElementData
{
	public BlobAssetReference<Clip> Clip;
}