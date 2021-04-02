using Unity.Animation;
using Unity.Entities;

public struct PlayerDeath_PlayClipRuntime : IComponentData
{
	public BlobAssetReference<Clip> clip;
}