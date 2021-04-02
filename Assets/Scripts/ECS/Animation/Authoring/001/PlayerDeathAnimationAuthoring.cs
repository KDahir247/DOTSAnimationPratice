using Unity.Animation.Hybrid;
using Unity.Entities;
using UnityEngine;

[ConverterVersion("PlayerDeathAnimationAuthoring", 1)] //if version changes then the whole scene get re-converted 
public sealed class PlayerDeathAnimationAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
	public AnimationClip clip;


	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
	{
		if (clip == null)
			return;

		conversionSystem.DeclareAssetDependency(gameObject,
			clip); //if clip changes then the dependent (this.gameObject) will get re-converted

		var clipRef = clip.ToDenseClip();
		conversionSystem.BlobAssetStore.AddUniqueBlobAsset(ref clipRef);
		dstManager.AddComponentData(entity, new PlayerDeath_PlayClipRuntime
		{
			clip = clipRef
		});

		dstManager.AddComponent<DeltaTimeRuntime>(entity);
	}
}