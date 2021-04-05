using Unity.Animation;
using Unity.Entities;

public struct RigRemapRuntime : IComponentData
{
	public BlobAssetReference<Clip> SrcClip;
	public BlobAssetReference<RigDefinition> SrcRig;
	public BlobAssetReference<RigRemapTable> RemapTable;
}