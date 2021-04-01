using Unity.Animation;
using Unity.Entities;

public struct RotateCube_PlayClipRuntime : IComponentData
{
	public BlobAssetReference<Clip> clip;
	public BlobAssetReference<AnimationCurveBlob> curve;
}
