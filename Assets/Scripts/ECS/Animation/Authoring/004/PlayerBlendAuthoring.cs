using System.Collections.Generic;
using Unity.Animation;
using Unity.Entities;
using UnityEngine;
using Unity.Animation.Hybrid;
[ConverterVersion("PlayerBlend",1)]
public class PlayerBlendAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
	[SerializeField]
	private AnimationClip clip01;
	[SerializeField]
	private AnimationClip clip02;

	private readonly List<BlobAssetReference<Clip>> clipsRef = new List<BlobAssetReference<Clip>>();

	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
	{
		if (clip01 == null || clip02 == null)
			return;

		clipsRef.Clear();

		conversionSystem.DeclareAssetDependency(gameObject, clip01);
		conversionSystem.DeclareAssetDependency(gameObject, clip02);

		BlobAssetReference<Clip> clipRef01 = clip01.ToDenseClip();
		BlobAssetReference<Clip> clipRef02 = clip02.ToDenseClip();

		conversionSystem.BlobAssetStore.AddUniqueBlobAsset(ref clipRef01);
		conversionSystem.BlobAssetStore.AddUniqueBlobAsset(ref clipRef02);

		clipsRef.Add(clipRef01);
		clipsRef.Add(clipRef02);

		DynamicBuffer<PlayerBlendClipBuffer> dynamicBufferClips = dstManager.AddBuffer<PlayerBlendClipBuffer>(entity);

		for (byte i = 0; i < clipsRef.Count; i++) { dynamicBufferClips.Add(new PlayerBlendClipBuffer() {Clip = clipsRef[i]}); }

		dstManager.AddComponent<PlayerBlendKernelRuntime>(entity); //Used for the animation node kernel.
		dstManager.AddComponent<DeltaTimeRuntime>(entity); //Used for the animation node kernel.
	}
}
