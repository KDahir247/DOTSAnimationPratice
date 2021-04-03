using Unity.Animation;
using Unity.Animation.Hybrid;
using Unity.Entities;
using Unity.Mathematics;
using UnityEditor.Animations;
using UnityEngine;

[ConverterVersion("BlendTree2D", 1)]
[RequireComponent(typeof(RigComponent))]
public sealed class BlendTree2DAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
	[SerializeField] private BlendTree blendTree;

	[SerializeField] private float2 paramStep;

	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
	{
		if (blendTree == null)
			return;

		conversionSystem.DeclareAssetDependency(gameObject, blendTree);

		var rig = dstManager.GetComponentData<Rig>(entity);

		var clipConfiguration = new ClipConfiguration
		{
			Mask = ClipConfigurationMask.LoopValues
		};

		var bakeOptions = new BakeOptions
		{
			ClipConfiguration = clipConfiguration,
			RigDefinition = rig.Value,
			SampleRate = 60.0f
		};

		var blendTreeIndex = BlendTreeConversion.Convert(blendTree, entity, dstManager, bakeOptions);
		var blendTree2DResources = dstManager.GetBuffer<BlendTree2DResource>(entity);

		var blendTreeAsset =
			BlendTreeBuilder.CreateBlendTree2DFromComponents(blendTree2DResources[blendTreeIndex], dstManager, entity);

		conversionSystem.BlobAssetStore.AddUniqueBlobAsset(ref blendTreeAsset);

		var blendTree2DData = new BlendTree2DRuntime
		{
			BlendTreeAsset = blendTreeAsset
		};

		dstManager.AddComponentData(entity, blendTree2DData);

		var blendTree2DParam = new BlendTree2DParamRuntime
		{
			InputMapping = float2.zero,
			StepMapping = paramStep
		};

		dstManager.AddComponentData(entity, blendTree2DParam);

		dstManager.AddComponent<DeltaTimeRuntime>(entity);
	}
}