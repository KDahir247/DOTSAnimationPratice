using System.Collections.Generic;
using Unity.Animation;
using Unity.Animation.Hybrid;
using Unity.Entities;
using UnityEngine;

[ConverterVersion("PlayerBlend", 1)]
public sealed class PlayerBlendAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
	[SerializeField] private AnimationClip clip01;

	[SerializeField] private AnimationClip clip02;

	private readonly List<BlobAssetReference<Clip>> _clipsRef = new List<BlobAssetReference<Clip>>();

	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
	{
		if (clip01 == null || clip02 == null)
			return;

		_clipsRef.Clear();

		conversionSystem.DeclareAssetDependency(gameObject, clip01);
		conversionSystem.DeclareAssetDependency(gameObject, clip02);

		var clipRef01 = clip01.ToDenseClip();
		var clipRef02 = clip02.ToDenseClip();

		conversionSystem.BlobAssetStore.AddUniqueBlobAsset(ref clipRef01);
		conversionSystem.BlobAssetStore.AddUniqueBlobAsset(ref clipRef02);

		_clipsRef.Add(clipRef01);
		_clipsRef.Add(clipRef02);

		var dynamicBufferClips = dstManager.AddBuffer<PlayerBlendClipBuffer>(entity);

		for (byte i = 0; i < _clipsRef.Count; i++)
			dynamicBufferClips.Add(new PlayerBlendClipBuffer {Clip = _clipsRef[i]});

		dstManager.AddComponent<PlayerBlendKernelRuntime>(entity); //Used for the animation node kernel.
		dstManager.AddComponent<DeltaTimeRuntime>(entity); //Used for the animation node kernel.
	}
}