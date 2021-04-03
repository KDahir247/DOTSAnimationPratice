using Unity.Animation;
using Unity.Entities;

public struct BlendTree2DRuntime : IComponentData
{
	public BlobAssetReference<BlendTree2DSimpleDirectional> BlendTreeAsset;
}