using Unity.Animation;
using Unity.Entities;

public struct BlendTree1DRuntime : IComponentData
{
	public BlobAssetReference<BlendTree1D> BlendTree;
}